using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// تفاصيل أسطر الحركة المخزنية
/// </summary>
public class InventoryTransactionLine : Entity
{
    public Guid InventoryTransactionMasterId { get; private set; }
    public virtual InventoryTransactionMaster? InventoryTransactionMaster { get; private set; }

    public Guid ItemId { get; private set; }
    public virtual Item? Item { get; private set; }

    public Guid UnitId { get; private set; }
    public virtual Unit? Unit { get; private set; }

    public decimal Quantity { get; private set; } // الكمية بالوحدة المختارة
    public decimal ConversionRate { get; private set; } // معامل التحويل وقت الحركة
    public decimal BaseQuantity { get; private set; } // الكمية بالوحدة الأساسية (Quantity * ConversionRate)
    
    public decimal Price { get; private set; } // سعر التكلفة أو البيع للسطر
    public decimal Total { get; private set; } // الإجمالي (BaseQuantity * Price)

    public string? BatchNumber { get; private set; } // رقم الدفعة المرتبط

    private InventoryTransactionLine() { } // For EF Core

    public InventoryTransactionLine(
        Guid id, 
        Guid masterId, 
        Guid itemId, 
        Guid unitId, 
        decimal quantity, 
        decimal conversionRate, 
        decimal price, 
        string? batchNumber = null) : base(id)
    {
        InventoryTransactionMasterId = masterId;
        ItemId = itemId;
        UnitId = unitId;
        Quantity = quantity;
        ConversionRate = conversionRate;
        BaseQuantity = quantity * conversionRate;
        Price = price;
        Total = BaseQuantity * price;
        BatchNumber = batchNumber;
    }
}
