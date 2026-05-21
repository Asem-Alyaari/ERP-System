using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

/// <summary>
/// رأس قيد اليومية
/// </summary>
public class JournalEntryMaster : Entity
{
    public string VoucherNumber { get; private set; } = string.Empty;
    public DateTime TransactionDate { get; private set; }
    public string? Description { get; private set; }
    
    public Guid FiscalPeriodId { get; private set; }
    public virtual FiscalPeriod? FiscalPeriod { get; private set; }

    public JournalEntryStatus Status { get; private set; }

    // Audit Fields
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public string? PostedBy { get; private set; }
    public DateTime? PostedAt { get; private set; }

    private readonly List<JournalEntryLine> _lines = new();
    public virtual IReadOnlyCollection<JournalEntryLine> Lines => _lines.AsReadOnly();

    private JournalEntryMaster() { } // For EF Core

    public JournalEntryMaster(
        Guid id, 
        string voucherNumber, 
        DateTime transactionDate, 
        string? description, 
        Guid fiscalPeriodId, 
        string createdBy) : base(id)
    {
        VoucherNumber = voucherNumber;
        TransactionDate = transactionDate;
        Description = description;
        FiscalPeriodId = fiscalPeriodId;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        Status = JournalEntryStatus.Draft;
    }

    public void Post(string postedBy)
    {
        Status = JournalEntryStatus.Posted;
        PostedBy = postedBy;
        PostedAt = DateTime.UtcNow;
    }

    public void Unpost()
    {
        Status = JournalEntryStatus.Draft;
        PostedBy = null;
        PostedAt = null;
    }

    public void Cancel()
    {
        Status = JournalEntryStatus.Cancelled;
    }
}
