using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// تفاصيل أسطر فاتورة المشتريات
/// </summary>
public class PurchaseInvoiceLine : Entity
{
    public Guid PurchaseInvoiceMasterId { get; private set; }
    public virtual PurchaseInvoiceMaster? PurchaseInvoiceMaster { get; private set; }

    public Guid ItemId { get; private set; }
    public virtual Item? Item { get; private set; }

    public Guid UnitId { get; private set; }
    public virtual Unit? Unit { get; private set; }

    public decimal Quantity { get; private set; }
    public decimal Price { get; private set; } // سعر الشراء في هذه الفاتورة

    public decimal TaxRate { get; private set; } // نسبة الضريبة (مثلاً 0.15)
    public decimal TaxAmount { get; private set; } // قيمة الضريبة للسطر
    public decimal Total { get; private set; } // الإجمالي شامل الضريبة

    public string? BatchNumber { get; private set; }
    public DateTime? ExpiryDate { get; private set; }

    private PurchaseInvoiceLine() { } // For EF Core

    public PurchaseInvoiceLine(
        Guid id, 
        Guid masterId, 
        Guid itemId, 
        Guid unitId, 
        decimal quantity, 
        decimal price, 
        decimal taxRate, 
        string? batchNumber = null, 
        DateTime? expiryDate = null) : base(id)
    {
        PurchaseInvoiceMasterId = masterId;
        ItemId = itemId;
        UnitId = unitId;
        Quantity = quantity;
        Price = price;
        TaxRate = taxRate;
        TaxAmount = quantity * price * taxRate;
        Total = (quantity * price) + TaxAmount;
        BatchNumber = batchNumber;
        ExpiryDate = expiryDate;
    }
}
