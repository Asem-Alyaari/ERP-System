using ERP.Application.Features.Accounting.AccountBalances.Specifications;
using ERP.Application.Features.Accounting.Reports.Queries.GetTrialBalance;
using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.FinancialReports.Queries.GetIncomeStatement;

public class GetIncomeStatementQueryHandler : IRequestHandler<GetIncomeStatementQuery, IncomeStatementDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetIncomeStatementQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IncomeStatementDto> Handle(GetIncomeStatementQuery request, CancellationToken cancellationToken)
    {
        // 1. جلب الفترة المالية
        var fiscalPeriod = await _unitOfWork.Repository<FiscalPeriod>()
            .GetByIdAsync(request.FiscalPeriodId);

        if (fiscalPeriod == null)
            throw new Exception("الفترة المالية غير موجودة.");

        // 2. جلب جميع الأرصدة للفترة المالية
        var balanceSpec = new TrialBalanceSpecification(
            request.FiscalPeriodId,
            request.CostCenterId,
            null
        );

        var balances = await _unitOfWork.Repository<AccountBalance>()
            .ListAsync(balanceSpec);

        // 3. جلب جميع الحسابات
        var accounts = await _unitOfWork.Repository<Account>()
            .ListAllAsync();

        // 4. تصفية الحسابات: الإيرادات (تبدأ بـ 4) والمصروفات (تبدأ بـ 5)
        var revenueAccounts = accounts.Where(a => a.AccountCode.StartsWith("4")).ToList();
        var expenseAccounts = accounts.Where(a => a.AccountCode.StartsWith("5")).ToList();

        // 5. تجميع الأرصدة لكل حساب
        var revenueLines = new List<IncomeStatementLineDto>();
        var expenseLines = new List<IncomeStatementLineDto>();

        foreach (var account in revenueAccounts)
        {
            var accountBalances = balances.Where(b => b.AccountId == account.Id).ToList();
            var totalBalance = accountBalances.Sum(b => b.CurrentBalance);

            revenueLines.Add(new IncomeStatementLineDto
            {
                AccountId = account.Id,
                AccountCode = account.AccountCode,
                AccountNameAr = account.AccountNameAr,
                AccountNameEn = account.AccountNameEn,
                Balance = totalBalance,
                Level = account.AccountCode.Length,
                ParentAccountId = account.ParentAccountId
            });
        }

        foreach (var account in expenseAccounts)
        {
            var accountBalances = balances.Where(b => b.AccountId == account.Id).ToList();
            var totalBalance = accountBalances.Sum(b => b.CurrentBalance);

            expenseLines.Add(new IncomeStatementLineDto
            {
                AccountId = account.Id,
                AccountCode = account.AccountCode,
                AccountNameAr = account.AccountNameAr,
                AccountNameEn = account.AccountNameEn,
                Balance = totalBalance,
                Level = account.AccountCode.Length,
                ParentAccountId = account.ParentAccountId
            });
        }

        // 6. حساب المجاميع
        var totalRevenues = revenueLines.Sum(r => r.Balance);
        var totalExpenses = expenseLines.Sum(e => e.Balance);
        var netProfitLoss = totalRevenues - totalExpenses;

        return new IncomeStatementDto
        {
            PeriodName = fiscalPeriod.YearName,
            FromDate = request.FromDate ?? fiscalPeriod.StartDate,
            ToDate = request.ToDate ?? fiscalPeriod.EndDate,
            Revenues = revenueLines.OrderBy(r => r.AccountCode).ToList(),
            Expenses = expenseLines.OrderBy(e => e.AccountCode).ToList(),
            TotalRevenues = totalRevenues,
            TotalExpenses = totalExpenses,
            NetProfitLoss = netProfitLoss
        };
    }
}
