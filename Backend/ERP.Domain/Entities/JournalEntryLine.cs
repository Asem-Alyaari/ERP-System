using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// تفاصيل أسطر قيد اليومية
/// </summary>
public class JournalEntryLine : Entity
{
    public Guid JournalEntryMasterId { get; private set; }
    public virtual JournalEntryMaster? JournalEntryMaster { get; private set; }

    public Guid AccountId { get; private set; }
    public virtual Account? Account { get; private set; }

    public decimal Debit { get; private set; } // المبلغ المدين بالعملة المحلية
    public decimal Credit { get; private set; } // المبلغ الدائن بالعملة المحلية

    public decimal? ForeignDebit { get; private set; } // المبالغ بالعملة الأجنبية
    public decimal? ForeignCredit { get; private set; }

    public Guid CurrencyId { get; private set; }
    public virtual Currency? Currency { get; private set; }

    public decimal ExchangeRate { get; private set; } // سعر الصرف أثناء الحركة

    public Guid? CostCenterId { get; private set; }
    public virtual CostCenter? CostCenter { get; private set; }

    public string? Memo { get; private set; } // شرح السطر

    private JournalEntryLine() { } // For EF Core

    public JournalEntryLine(
        Guid id, 
        Guid journalEntryMasterId, 
        Guid accountId, 
        decimal debit, 
        decimal credit, 
        Guid currencyId, 
        decimal exchangeRate, 
        Guid? costCenterId = null, 
        string? memo = null,
        decimal? foreignDebit = null,
        decimal? foreignCredit = null) : base(id)
    {
        JournalEntryMasterId = journalEntryMasterId;
        AccountId = accountId;
        Debit = debit;
        Credit = credit;
        CurrencyId = currencyId;
        ExchangeRate = exchangeRate;
        CostCenterId = costCenterId;
        Memo = memo;
        ForeignDebit = foreignDebit;
        ForeignCredit = foreignCredit;
    }
}
