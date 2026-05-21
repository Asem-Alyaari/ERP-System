using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// تفاصيل أسطر فاتورة المبيعات
/// </summary>
public class SalesInvoiceLine : Entity
{
    public Guid SalesInvoiceMasterId { get; private set; }
    public virtual SalesInvoiceMaster? SalesInvoiceMaster { get; private set; }

    public Guid ItemId { get; private set; }
    public virtual Item? Item { get; private set; }

    public Guid UnitId { get; private set; }
    public virtual Unit? Unit { get; private set; }

    public decimal Quantity { get; private set; }
    public decimal Price { get; private set; } // سعر البيع في هذه الفاتورة

    public decimal TaxRate { get; private set; } // نسبة الضريبة (مثلاً 0.15)
    public decimal TaxAmount { get; private set; } // قيمة الضريبة للسطر
    public decimal Total { get; private set; } // الإجمالي شامل الضريبة

    public string? BatchNumber { get; private set; } // رقم الدفعة المصروف منها

    private SalesInvoiceLine() { } // For EF Core

    public SalesInvoiceLine(
        Guid id, 
        Guid masterId, 
        Guid itemId, 
        Guid unitId, 
        decimal quantity, 
        decimal price, 
        decimal taxRate, 
        string? batchNumber = null) : base(id)
    {
        SalesInvoiceMasterId = masterId;
        ItemId = itemId;
        UnitId = unitId;
        Quantity = quantity;
        Price = price;
        TaxRate = taxRate;
        TaxAmount = quantity * price * taxRate;
        Total = (quantity * price) + TaxAmount;
        BatchNumber = batchNumber;
    }
}
