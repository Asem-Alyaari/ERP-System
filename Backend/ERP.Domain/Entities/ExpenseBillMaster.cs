using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

/// <summary>
/// فاتورة المصروفات (موديول المصروفات وفواتير الخدمات)
/// </summary>
public class ExpenseBillMaster : Entity
{
    public string BillNumber { get; private set; } = string.Empty;
    public DateTime TransactionDate { get; private set; }
    public ExpenseBillPaymentMethod PaymentMethod { get; private set; }

    // المورد (اختياري للمصروفات النقدية، إلزامي للمصروفات الآجلة)
    public Guid? VendorId { get; private set; }
    public virtual Vendor? Vendor { get; private set; }

    // اسم المورد (للمصروفات النقدية التي لا تحتاج لإنشاء مورد)
    public string? SupplierName { get; private set; }

    // الحساب الذي سيتم الدفع منه (للمصروفات النقدية والبنكية)
    public Guid? PaymentAccountId { get; private set; }
    public virtual Account? PaymentAccount { get; private set; }

    public decimal TotalAmount { get; private set; }
    public decimal TaxAmount { get; private set; }
    public decimal NetAmount { get; private set; }
    public string? Notes { get; private set; }
    public ExpenseBillStatus Status { get; private set; }

    // Audit Fields
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public string? PostedBy { get; private set; }
    public DateTime? PostedAt { get; private set; }

    private readonly List<ExpenseBillLine> _lines = new();
    public virtual IReadOnlyCollection<ExpenseBillLine> Lines => _lines.AsReadOnly();

    private ExpenseBillMaster() { } // For EF Core

    public ExpenseBillMaster(
        Guid id,
        string billNumber,
        DateTime transactionDate,
        ExpenseBillPaymentMethod paymentMethod,
        decimal totalAmount,
        decimal taxAmount,
        decimal netAmount,
        string createdBy,
        string? notes = null,
        Guid? vendorId = null,
        string? supplierName = null,
        Guid? paymentAccountId = null) : base(id)
    {
        BillNumber = billNumber;
        TransactionDate = transactionDate;
        PaymentMethod = paymentMethod;
        TotalAmount = totalAmount;
        TaxAmount = taxAmount;
        NetAmount = netAmount;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        Notes = notes;
        VendorId = vendorId;
        SupplierName = supplierName;
        PaymentAccountId = paymentAccountId;
        Status = ExpenseBillStatus.Draft;
    }

    public void Post(string postedBy)
    {
        Status = ExpenseBillStatus.Posted;
        PostedBy = postedBy;
        PostedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = ExpenseBillStatus.Cancelled;
    }

    public void AddLine(ExpenseBillLine line)
    {
        _lines.Add(line);
    }
}
