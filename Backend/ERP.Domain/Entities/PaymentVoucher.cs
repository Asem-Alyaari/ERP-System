using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

/// <summary>
/// سند الصرف (دفع أموال لمورد أو مصروف)
/// </summary>
public class PaymentVoucher : Entity
{
    public string VoucherNumber { get; private set; } = string.Empty;
    public DateTime VoucherDate { get; private set; }
    public PaymentMethod PaymentMethod { get; private set; }

    // الحساب الذي خرجت منه الأموال (دائن - خزينة أو بنك)
    public Guid SourceAccountId { get; private set; }
    public virtual Account? SourceAccount { get; private set; }

    public VoucherPartnerType DestinationType { get; private set; }

    // الوجهة التي استلمت الأموال (مدين)
    public Guid? VendorId { get; private set; }
    public virtual Vendor? Vendor { get; private set; }

    public Guid? DestinationAccountId { get; private set; }
    public virtual Account? DestinationAccount { get; private set; }

    public decimal Amount { get; private set; }
    public string? Notes { get; private set; }
    public VoucherStatus Status { get; private set; }

    // مركز التكلفة (للمصروفات التي تتطلب مركز تكلفة)
    public Guid? CostCenterId { get; private set; }
    public virtual CostCenter? CostCenter { get; private set; }

    // Audit Fields
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public string? PostedBy { get; private set; }
    public DateTime? PostedAt { get; private set; }

    private PaymentVoucher() { } // For EF Core

    public PaymentVoucher(
        Guid id,
        string voucherNumber,
        DateTime voucherDate,
        PaymentMethod paymentMethod,
        Guid sourceAccountId,
        VoucherPartnerType destinationType,
        decimal amount,
        string createdBy,
        string? notes = null,
        Guid? vendorId = null,
        Guid? destinationAccountId = null,
        Guid? costCenterId = null) : base(id)
    {
        VoucherNumber = voucherNumber;
        VoucherDate = voucherDate;
        PaymentMethod = paymentMethod;
        SourceAccountId = sourceAccountId;
        DestinationType = destinationType;
        Amount = amount;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        Notes = notes;
        VendorId = vendorId;
        DestinationAccountId = destinationAccountId;
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
