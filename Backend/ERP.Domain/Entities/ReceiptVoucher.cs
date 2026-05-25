using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

/// <summary>
/// سند القبض (تحصيل أموال من عميل أو حساب)
/// </summary>
public class ReceiptVoucher : Entity
{
    public string VoucherNumber { get; private set; } = string.Empty;
    public DateTime VoucherDate { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }

    // الحساب الذي استلم الأموال (مدين - خزينة أو بنك)
    public Guid DestinationAccountId { get; private set; }
    public virtual Account? DestinationAccount { get; private set; }

    public VoucherPartnerType SourceType { get; private set; }

    // المصدر الذي دفع الأموال (دائن)
    public Guid? CustomerId { get; private set; }
    public virtual Customer? Customer { get; private set; }

    public Guid? VendorId { get; private set; }
    public virtual Vendor? Vendor { get; private set; }

    public Guid? SourceAccountId { get; private set; }
    public virtual Account? SourceAccount { get; private set; }

    public decimal Amount { get; private set; }
    public string? Notes { get; private set; }
    public VoucherStatus Status { get; private set; }

    public Guid? CostCenterId { get; private set; }
    public virtual CostCenter? CostCenter { get; private set; }

    // Audit Fields
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public string? PostedBy { get; private set; }
    public DateTime? PostedAt { get; private set; }

    private ReceiptVoucher() { } // For EF Core

    public ReceiptVoucher(
        Guid id, 
        string voucherNumber, 
        DateTime voucherDate, 
        PaymentMethod paymentMethod, 
        Guid destinationAccountId, 
        VoucherPartnerType sourceType, 
        decimal amount, 
        string createdBy, 
        string? notes = null, 
        Guid? customerId = null, 
        Guid? vendorId = null,
        Guid? sourceAccountId = null,
        Guid? costCenterId = null) : base(id)
    {
        VoucherNumber = voucherNumber;
        VoucherDate = voucherDate;
        PaymentMethod = paymentMethod;
        DestinationAccountId = destinationAccountId;
        SourceType = sourceType;
        Amount = amount;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        Notes = notes;
        CustomerId = customerId;
        VendorId = vendorId;
        SourceAccountId = sourceAccountId;
        CostCenterId = costCenterId;
        Status = VoucherStatus.Draft;
    }

    public void Post(string postedBy)
    {
        Status = VoucherStatus.Posted;
        PostedBy = postedBy;
        PostedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = VoucherStatus.Cancelled;
    }
}
