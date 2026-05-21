using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// وحدات الصنف (ربط الصنف بالوحدات ومعاملات التحويل)
/// </summary>
public class ItemUnit : Entity
{
    public Guid ItemId { get; private set; }
    public virtual Item? Item { get; private set; }

    public Guid UnitId { get; private set; }
    public virtual Unit? Unit { get; private set; }

    public decimal ConversionRate { get; private set; } // معامل التحويل بالنسبة للوحدة الأساسية
    public bool IsBaseUnit { get; private set; } // هل هي الوحدة الأصغر؟
    public decimal Price { get; private set; } // سعر البيع الافتراضي لهذه الوحدة

    private ItemUnit() { } // For EF Core

    public ItemUnit(
        Guid id, 
        Guid itemId, 
        Guid unitId, 
        decimal conversionRate, 
        bool isBaseUnit, 
        decimal price = 0) : base(id)
    {
        ItemId = itemId;
        UnitId = unitId;
        ConversionRate = conversionRate;
        IsBaseUnit = isBaseUnit;
        Price = price;
    }
}
