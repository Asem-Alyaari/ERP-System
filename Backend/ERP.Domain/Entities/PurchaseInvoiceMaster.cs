using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

/// <summary>
/// رأس فاتورة المشتريات
/// </summary>
public class PurchaseInvoiceMaster : Entity
{
    public string InvoiceNumber { get; private set; } = string.Empty; // رقم آلي
    public string? VendorInvoiceNumber { get; private set; } // رقم فاتورة المورد يدوي
    public DateTime InvoiceDate { get; private set; }
    
    public Guid VendorId { get; private set; }
    public virtual Vendor? Vendor { get; private set; }

    public Guid FiscalPeriodId { get; private set; }
    public virtual FiscalPeriod? FiscalPeriod { get; private set; }

    public PaymentType PaymentType { get; private set; }
    
    public decimal TotalAmount { get; private set; } // الإجمالي قبل الضريبة
    public decimal TaxAmount { get; private set; } // إجمالي الضريبة
    public decimal NetAmount { get; private set; } // الصافي النهائي

    public PurchaseInvoiceStatus Status { get; private set; }

    // Audit Fields
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public string? PostedBy { get; private set; }
    public DateTime? PostedAt { get; private set; }

    private readonly List<PurchaseInvoiceLine> _lines = new();
    public virtual IReadOnlyCollection<PurchaseInvoiceLine> Lines => _lines.AsReadOnly();

    private PurchaseInvoiceMaster() { } // For EF Core

    public PurchaseInvoiceMaster(
        Guid id, 
        string invoiceNumber, 
        DateTime invoiceDate, 
        Guid vendorId, 
        Guid fiscalPeriodId, 
        PaymentType paymentType, 
        string createdBy, 
        string? vendorInvoiceNumber = null) : base(id)
    {
        InvoiceNumber = invoiceNumber;
        InvoiceDate = invoiceDate;
        VendorId = vendorId;
        FiscalPeriodId = fiscalPeriodId;
        PaymentType = paymentType;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        VendorInvoiceNumber = vendorInvoiceNumber;
        Status = PurchaseInvoiceStatus.Draft;
    }

    public void UpdateTotals(decimal totalAmount, decimal taxAmount)
    {
        TotalAmount = totalAmount;
        TaxAmount = taxAmount;
        NetAmount = totalAmount + taxAmount;
    }

    public void Post(string postedBy)
    {
        Status = PurchaseInvoiceStatus.Posted;
        PostedBy = postedBy;
        PostedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = PurchaseInvoiceStatus.Cancelled;
    }
}
