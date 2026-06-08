using ERP.Application.Features.Accounting.JournalEntries.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using ERP.Domain.Services;
using MediatR;

namespace ERP.Application.Features.Accounting.FiscalPeriods.Commands.Close;

public record CloseFiscalPeriodCommand(Guid Id, string ClosedBy) : IRequest<bool>;

public class CloseFiscalPeriodCommandHandler : IRequestHandler<CloseFiscalPeriodCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountBalanceService _accountBalanceService;

    public CloseFiscalPeriodCommandHandler(
        IUnitOfWork unitOfWork,
        IAccountBalanceService accountBalanceService)
    {
        _unitOfWork = unitOfWork;
        _accountBalanceService = accountBalanceService;
    }

    public async Task<bool> Handle(CloseFiscalPeriodCommand request, CancellationToken cancellationToken)
    {
        // 1. جلب الفترة المالية
        var period = await _unitOfWork.Repository<FiscalPeriod>().GetByIdAsync(request.Id);
        if (period == null)
            throw new BusinessException("الفترة المالية غير موجودة.");

        // 2. التحقق من أن الفترة ليست مغلقة بالفعل
        if (period.IsClosed)
            throw new BusinessException("الفترة المالية مغلقة بالفعل.");

        // 3. جلب جميع الأرصدة لهذه الفترة
        var balances = await _unitOfWork.Repository<AccountBalance>()
            .ListAllAsync();

        var periodBalances = balances.Where(b => b.FiscalPeriodId == request.Id).ToList();

        // 4. حساب صافي الربح/الخسارة
        // الإيرادات (4xxx) - المصروفات (5xxx)
        var revenueAccounts = await _unitOfWork.Repository<Account>()
            .ListAllAsync();
        var revenueAccountIds = revenueAccounts
            .Where(a => a.AccountType == AccountType.Revenue && a.IsDetail)
            .Select(a => a.Id)
            .ToHashSet();

        var expenseAccountIds = revenueAccounts
            .Where(a => a.AccountType == AccountType.Expense && a.IsDetail)
            .Select(a => a.Id)
            .ToHashSet();

        decimal totalRevenues = 0;
        decimal totalExpenses = 0;

        foreach (var balance in periodBalances)
        {
            if (revenueAccountIds.Contains(balance.AccountId))
            {
                // الإيرادات: الرصيد الدائن يمثل الإيرادات
                totalRevenues += balance.TotalCredit - balance.TotalDebit;
            }
            else if (expenseAccountIds.Contains(balance.AccountId))
            {
                // المصروفات: الرصيد المدين يمثل المصروفات
                totalExpenses += balance.TotalDebit - balance.TotalCredit;
            }
        }

        decimal netProfitLoss = totalRevenues - totalExpenses;

        // 5. إنشاء قيد إغلاق الفترة المالية
        var closingEntryId = Guid.NewGuid();
        var voucherNumber = $"CL-{period.YearName}-{DateTime.Now:yyyyMMddHHmmss}";
        var closingEntry = new JournalEntryMaster(
            closingEntryId,
            voucherNumber,
            period.EndDate,
            $"قيد إغلاق الفترة المالية {period.YearName}",
            period.Id,
            request.ClosedBy);

        _unitOfWork.Repository<JournalEntryMaster>().Add(closingEntry);

        // 6. إنشاء أسطر القيد لتصفير حسابات الإيرادات والمصروفات
        var localCurrency = (await _unitOfWork.Repository<Currency>()
            .ListAllAsync()).FirstOrDefault(c => c.IsLocal);

        if (localCurrency == null)
            throw new BusinessException("العملة المحلية غير موجودة.");

        // تصفير حسابات الإيرادات (4xxx)
        foreach (var balance in periodBalances.Where(b => revenueAccountIds.Contains(b.AccountId)))
        {
            var netRevenue = balance.TotalCredit - balance.TotalDebit;
            if (netRevenue > 0)
            {
                // Debit الإيرادات لتصفيرها
                var line = new JournalEntryLine(
                    Guid.NewGuid(),
                    closingEntryId,
                    balance.AccountId,
                    netRevenue, // Debit
                    0,         // Credit
                    localCurrency.Id,
                    1,         // Exchange rate
                    balance.CostCenterId,
                    "تصفير حساب الإيرادات عند إغلاق الفترة"
                );
                _unitOfWork.Repository<JournalEntryLine>().Add(line);
            }
        }

        // تصفير حسابات المصروفات (5xxx)
        foreach (var balance in periodBalances.Where(b => expenseAccountIds.Contains(b.AccountId)))
        {
            var netExpense = balance.TotalDebit - balance.TotalCredit;
            if (netExpense > 0)
            {
                // Credit المصروفات لتصفيرها
                var line = new JournalEntryLine(
                    Guid.NewGuid(),
                    closingEntryId,
                    balance.AccountId,
                    0,         // Debit
                    netExpense, // Credit
                    localCurrency.Id,
                    1,         // Exchange rate
                    balance.CostCenterId,
                    "تصفير حساب المصروفات عند إغلاق الفترة"
                );
                _unitOfWork.Repository<JournalEntryLine>().Add(line);
            }
        }

        // 7. نقل صافي الربح/الخسارة إلى حساب الأرباح المحتجزة (3xxx)
        var retainedEarningsAccount = revenueAccounts
            .FirstOrDefault(a => a.AccountType == AccountType.Equity && 
                                 a.IsDetail && 
                                 a.AccountNameAr.Contains("الأرباح المحتجزة"));

        if (retainedEarningsAccount == null)
            throw new BusinessException("حساب الأرباح المحتجزة غير موجود في دليل الحسابات.");

        if (netProfitLoss > 0)
        {
            // ربح: Credit الأرباح المحتجزة
            var profitLine = new JournalEntryLine(
                Guid.NewGuid(),
                closingEntryId,
                retainedEarningsAccount.Id,
                0,              // Debit
                netProfitLoss,  // Credit
                localCurrency.Id,
                1,
                null,
                "صافي الربح للفترة"
            );
            _unitOfWork.Repository<JournalEntryLine>().Add(profitLine);
        }
        else if (netProfitLoss < 0)
        {
            // خسارة: Debit الأرباح المحتجزة
            var lossLine = new JournalEntryLine(
                Guid.NewGuid(),
                closingEntryId,
                retainedEarningsAccount.Id,
                Math.Abs(netProfitLoss), // Debit
                0,                        // Credit
                localCurrency.Id,
                1,
                null,
                "صافي الخسارة للفترة"
            );
            _unitOfWork.Repository<JournalEntryLine>().Add(lossLine);
        }

        // 8. ترحيل قيد الإغلاق
        closingEntry.Post(request.ClosedBy);
        _unitOfWork.Repository<JournalEntryMaster>().Update(closingEntry);

        // 9. تحديث الأرصدة لأسطر قيد الإغلاق
        await _accountBalanceService.UpdateBalancesForLinesAsync(
            closingEntry.Lines,
            period.Id,
            cancellationToken);

        // 10. إغلاق الفترة المالية
        period.ClosePeriod();
        _unitOfWork.Repository<FiscalPeriod>().Update(period);

        // 11. حفظ التغييرات (TransactionBehavior سيتولى إدارة Transaction)
        await _unitOfWork.Complete();

        return true;
    }
}
