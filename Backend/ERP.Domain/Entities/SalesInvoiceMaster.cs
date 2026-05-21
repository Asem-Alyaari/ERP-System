using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

/// <summary>
/// رأس فاتورة المبيعات
/// </summary>
public class SalesInvoiceMaster : Entity
{
    public string InvoiceNumber { get; private set; } = string.Empty;
    public DateTime InvoiceDate { get; private set; }
    
    public Guid CustomerId { get; private set; }
    public virtual Customer? Customer { get; private set; }

    public Guid FiscalPeriodId { get; private set; }
    public virtual FiscalPeriod? FiscalPeriod { get; private set; }

    public PaymentType PaymentType { get; private set; }
    
    public decimal TotalAmount { get; private set; } // الإجمالي قبل الضريبة
    public decimal TaxAmount { get; private set; } // إجمالي الضريبة
    public decimal NetAmount { get; private set; } // الصافي النهائي

    public SalesInvoiceStatus Status { get; private set; }

    // Audit Fields
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public string? PostedBy { get; private set; }
    public DateTime? PostedAt { get; private set; }

    private readonly List<SalesInvoiceLine> _lines = new();
    public virtual IReadOnlyCollection<SalesInvoiceLine> Lines => _lines.AsReadOnly();

    private SalesInvoiceMaster() { } // For EF Core

    public SalesInvoiceMaster(
        Guid id, 
        string invoiceNumber, 
        DateTime invoiceDate, 
        Guid customerId, 
        Guid fiscalPeriodId, 
        PaymentType paymentType, 
        string createdBy) : base(id)
    {
        InvoiceNumber = invoiceNumber;
        InvoiceDate = invoiceDate;
        CustomerId = customerId;
        FiscalPeriodId = fiscalPeriodId;
        PaymentType = paymentType;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        Status = SalesInvoiceStatus.Draft;
    }

    public void UpdateTotals(decimal totalAmount, decimal taxAmount)
    {
        TotalAmount = totalAmount;
        TaxAmount = taxAmount;
        NetAmount = totalAmount + taxAmount;
    }

    public void Post(string postedBy)
    {
        Status = SalesInvoiceStatus.Posted;
        PostedBy = postedBy;
        PostedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = SalesInvoiceStatus.Cancelled;
    }
}
