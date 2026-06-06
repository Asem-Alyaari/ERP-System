using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using ERP.Domain.Services;
using MediatR;

namespace ERP.Application.Features.Accounting.OpeningBalances.Commands;

public class SetOpeningBalancesCommandHandler : IRequestHandler<SetOpeningBalancesCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountBalanceService _accountBalanceService;

    public SetOpeningBalancesCommandHandler(
        IUnitOfWork unitOfWork,
        IAccountBalanceService accountBalanceService)
    {
        _unitOfWork = unitOfWork;
        _accountBalanceService = accountBalanceService;
    }

    public async Task<bool> Handle(SetOpeningBalancesCommand request, CancellationToken cancellationToken)
    {
        // 1. التحقق من وجود الفترة المالية
        var fiscalPeriod = await _unitOfWork.Repository<FiscalPeriod>()
            .GetByIdAsync(request.FiscalPeriodId);

        if (fiscalPeriod == null)
            throw new BusinessException("الفترة المالية غير موجودة.");

        // 2. التحقق من أن الفترة مفتوحة
        if (fiscalPeriod.IsClosed)
            throw new BusinessException("لا يمكن تعيين أرصدة افتتاحية لفترة مالية مغلقة.");

        // 3. التحقق من توازن الأرصدة
        var validation = _accountBalanceService.ValidateOpeningBalances(
            request.Balances.Select(b => (
                b.AccountId,
                b.Debit,
                b.Credit,
                b.CostCenterId,
                b.CurrencyId // Keep as nullable
            )));

        if (!validation.IsBalanced)
        {
            throw new BusinessException(
                $"الأرصدة الافتتاحية غير متوازنة. إجمالي المدين: {validation.TotalDebit}، إجمالي الدائن: {validation.TotalCredit}");
        }

        // 4. الحصول على العملة المحلية الافتراضية
        var currencies = await _unitOfWork.Repository<Currency>()
            .ListAllAsync();
        var localCurrency = currencies.FirstOrDefault(c => c.IsLocal);

        if (localCurrency == null)
            throw new BusinessException("العملة المحلية غير موجودة.");

        // 5. تعيين الأرصدة الافتتاحية
        foreach (var balance in request.Balances)
        {
            // التحقق من وجود الحساب
            var account = await _unitOfWork.Repository<Account>()
                .GetByIdAsync(balance.AccountId);

            if (account == null)
                throw new BusinessException($"الحساب {balance.AccountId} غير موجود.");

            // استخدام العملة المحلية إذا لم يتم تحديد عملة
            var currencyId = balance.CurrencyId ?? localCurrency.Id;

            // تعيين الرصيد الافتتاحي
            await _accountBalanceService.SetOpeningBalanceAsync(
                balance.AccountId,
                request.FiscalPeriodId,
                balance.Debit,
                balance.Credit,
                balance.CostCenterId,
                currencyId);
        }

        // 6. حفظ التغييرات
        await _unitOfWork.Complete();

        return true;
    }
}
