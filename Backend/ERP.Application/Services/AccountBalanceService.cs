using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Domain.Services;
using ERP.Application.Features.Accounting.AccountBalances.Specifications;
using Microsoft.Extensions.Logging;

namespace ERP.Application.Services;

/// <summary>
/// تنفيذ خدمة تحديث الأرصدة المحاسبية المركزية
/// </summary>
public class AccountBalanceService : IAccountBalanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AccountBalanceService> _logger;

    public AccountBalanceService(
        IUnitOfWork unitOfWork,
        ILogger<AccountBalanceService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task UpdateBalanceForLineAsync(
        JournalEntryLine line,
        Guid fiscalPeriodId,
        CancellationToken cancellationToken = default)
    {
        var balanceSpec = new AccountBalanceFilterSpecification(
            fiscalPeriodId,
            line.AccountId,
            line.CostCenterId,
            line.CurrencyId
        );

        var balance = await _unitOfWork.Repository<AccountBalance>()
            .GetEntityWithSpec(balanceSpec);

        if (balance == null)
        {
            // إنشاء سجل رصيد جديد
            balance = new AccountBalance(
                Guid.NewGuid(),
                fiscalPeriodId,
                line.AccountId,
                line.CurrencyId,
                line.CostCenterId
            );

            balance.AddTransaction(line.Debit, line.Credit);
            _unitOfWork.Repository<AccountBalance>().Add(balance);

            _logger.LogInformation(
                "Created new AccountBalance for Account {AccountId}, Period {PeriodId}",
                line.AccountId, fiscalPeriodId);
        }
        else
        {
            // تحديث السجل الموجود
            balance.AddTransaction(line.Debit, line.Credit);
            _unitOfWork.Repository<AccountBalance>().Update(balance);

            _logger.LogDebug(
                "Updated AccountBalance for Account {AccountId}, Period {PeriodId}",
                line.AccountId, fiscalPeriodId);
        }
    }

    public async Task UpdateBalancesForLinesAsync(
        IEnumerable<JournalEntryLine> lines,
        Guid fiscalPeriodId,
        CancellationToken cancellationToken = default)
    {
        foreach (var line in lines)
        {
            await UpdateBalanceForLineAsync(line, fiscalPeriodId, cancellationToken);
        }
    }

    public async Task ReverseBalanceForLineAsync(
        JournalEntryLine line,
        Guid fiscalPeriodId,
        CancellationToken cancellationToken = default)
    {
        var balanceSpec = new AccountBalanceFilterSpecification(
            fiscalPeriodId,
            line.AccountId,
            line.CostCenterId,
            line.CurrencyId
        );

        var balance = await _unitOfWork.Repository<AccountBalance>()
            .GetEntityWithSpec(balanceSpec);

        if (balance != null)
        {
            // طرح المبالغ من الرصيد التراكمي
            balance.SubtractTransaction(line.Debit, line.Credit);
            _unitOfWork.Repository<AccountBalance>().Update(balance);

            _logger.LogInformation(
                "Reversed AccountBalance for Account {AccountId}, Period {PeriodId}",
                line.AccountId, fiscalPeriodId);
        }
        else
        {
            _logger.LogWarning(
                "AccountBalance not found for reversal - Account {AccountId}, Period {PeriodId}",
                line.AccountId, fiscalPeriodId);
        }
    }

    public async Task ReverseBalancesForLinesAsync(
        IEnumerable<JournalEntryLine> lines,
        Guid fiscalPeriodId,
        CancellationToken cancellationToken = default)
    {
        foreach (var line in lines)
        {
            await ReverseBalanceForLineAsync(line, fiscalPeriodId, cancellationToken);
        }
    }

    public async Task SetOpeningBalanceAsync(
        Guid accountId,
        Guid fiscalPeriodId,
        decimal debit,
        decimal credit,
        Guid? costCenterId = null,
        Guid? currencyId = null,
        CancellationToken cancellationToken = default)
    {
        var balanceSpec = new AccountBalanceFilterSpecification(
            fiscalPeriodId,
            accountId,
            costCenterId,
            currencyId
        );

        var balance = await _unitOfWork.Repository<AccountBalance>()
            .GetEntityWithSpec(balanceSpec);

        if (balance == null)
        {
            // إنشاء سجل رصيد جديد بالأرصدة الافتتاحية
            balance = new AccountBalance(
                Guid.NewGuid(),
                fiscalPeriodId,
                accountId,
                currencyId ?? Guid.Empty,
                costCenterId
            );

            // تعيين الأرصدة مباشرة
            balance.GetType()
                .GetProperty(nameof(AccountBalance.TotalDebit))?
                .SetValue(balance, debit);

            balance.GetType()
                .GetProperty(nameof(AccountBalance.TotalCredit))?
                .SetValue(balance, credit);

            balance.GetType()
                .GetProperty(nameof(AccountBalance.CurrentBalance))?
                .SetValue(balance, debit - credit);

            _unitOfWork.Repository<AccountBalance>().Add(balance);

            _logger.LogInformation(
                "Set opening balance for Account {AccountId}, Period {PeriodId}: Debit={Debit}, Credit={Credit}",
                accountId, fiscalPeriodId, debit, credit);
        }
        else
        {
            throw new InvalidOperationException(
                $"Opening balance already exists for Account {accountId} in Period {fiscalPeriodId}");
        }
    }

    public (bool IsBalanced, decimal TotalDebit, decimal TotalCredit) ValidateOpeningBalances(
        IEnumerable<(Guid AccountId, decimal Debit, decimal Credit, Guid? CostCenterId, Guid? CurrencyId)> balances)
    {
        var totalDebit = balances.Sum(b => b.Debit);
        var totalCredit = balances.Sum(b => b.Credit);

        var isBalanced = Math.Abs(totalDebit - totalCredit) < 0.01m;

        return (isBalanced, totalDebit, totalCredit);
    }
}
