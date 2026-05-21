using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// أرصدة الحسابات التراكمية (لفترة مالية، حساب، مركز تكلفة، وعملة معينة)
/// </summary>
public class AccountBalance : Entity
{
    public Guid FiscalPeriodId { get; private set; }
    public virtual FiscalPeriod? FiscalPeriod { get; private set; }

    public Guid AccountId { get; private set; }
    public virtual Account? Account { get; private set; }

    public Guid? CostCenterId { get; private set; }
    public virtual CostCenter? CostCenter { get; private set; }

    public Guid CurrencyId { get; private set; }
    public virtual Currency? Currency { get; private set; }

    public decimal TotalDebit { get; private set; }
    public decimal TotalCredit { get; private set; }
    public decimal CurrentBalance { get; private set; }

    private AccountBalance() { } // For EF Core

    public AccountBalance(
        Guid id, 
        Guid fiscalPeriodId, 
        Guid accountId, 
        Guid currencyId, 
        Guid? costCenterId = null) : base(id)
    {
        FiscalPeriodId = fiscalPeriodId;
        AccountId = accountId;
        CurrencyId = currencyId;
        CostCenterId = costCenterId;
        TotalDebit = 0;
        TotalCredit = 0;
        CurrentBalance = 0;
    }

    public void AddTransaction(decimal debit, decimal credit)
    {
        TotalDebit += debit;
        TotalCredit += credit;
        CurrentBalance = TotalDebit - TotalCredit;
    }

    public void SubtractTransaction(decimal debit, decimal credit)
    {
        TotalDebit -= debit;
        TotalCredit -= credit;
        CurrentBalance = TotalDebit - TotalCredit;
    }
}
