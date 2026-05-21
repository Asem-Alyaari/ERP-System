using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// الأصناف (المنتجات)
/// </summary>
public class Item : Entity
{
    public string ItemCode { get; private set; } = string.Empty;
    public string ItemNameAr { get; private set; } = string.Empty;
    public string ItemNameEn { get; private set; } = string.Empty;

    public Guid StockGroupId { get; private set; }
    public virtual StockGroup? StockGroup { get; private set; }

    public Guid? CategoryId { get; private set; }
    public virtual Category? Category { get; private set; }

    public string? Sku { get; private set; }
    public string? Barcode { get; private set; }

    public decimal DefaultPurchasePrice { get; private set; } // سعر الشراء الافتراضي (استرشادي)
    public decimal SalesPrice { get; private set; }

    public bool IsActive { get; private set; }

    // حدود الأمان والتنبيه المخزني
    public decimal ReorderLevel { get; private set; }    // حد إعادة الطلب
    public decimal MinimumQuantity { get; private set; }  // الحد الأدنى
    public decimal MaximumQuantity { get; private set; }  // الحد الأعلى
    
    public Guid? SafetyStockUnitId { get; private set; } // الوحدة التي تم ضبط الحدود بها
    public virtual Unit? SafetyStockUnit { get; private set; }

    private readonly List<ItemUnit> _itemUnits = new();
    public virtual IReadOnlyCollection<ItemUnit> ItemUnits => _itemUnits;

    private readonly List<ItemBatch> _batches = new();
    public virtual IReadOnlyCollection<ItemBatch> Batches => _batches;

    private Item() { } // For EF Core

    public Item(
        Guid id, 
        string itemCode, 
        string itemNameAr, 
        string itemNameEn, 
        Guid stockGroupId, 
        string? sku = null, 
        string? barcode = null, 
        decimal defaultPurchasePrice = 0, 
        decimal salesPrice = 0) : base(id)
    {
        ItemCode = itemCode;
        ItemNameAr = itemNameAr;
        ItemNameEn = itemNameEn;
        StockGroupId = stockGroupId;
        Sku = sku;
        Barcode = barcode;
        DefaultPurchasePrice = defaultPurchasePrice;
        SalesPrice = salesPrice;
        IsActive = true;
    }

    public void Update(
        string itemCode, 
        string itemNameAr, 
        string itemNameEn, 
        Guid stockGroupId, 
        string? sku = null, 
        string? barcode = null, 
        decimal defaultPurchasePrice = 0, 
        decimal salesPrice = 0)
    {
        ItemCode = itemCode;
        ItemNameAr = itemNameAr;
        ItemNameEn = itemNameEn;
        StockGroupId = stockGroupId;
        Sku = sku;
        Barcode = barcode;
        DefaultPurchasePrice = defaultPurchasePrice;
        SalesPrice = salesPrice;
    }


    public void SetInventoryLimits(decimal reorderLevel, decimal minQty, decimal maxQty, Guid unitId)
    {
        ReorderLevel = reorderLevel;
        MinimumQuantity = minQty;
        MaximumQuantity = maxQty;
        SafetyStockUnitId = unitId;
    }

    public void SetCategory(Guid? categoryId)
    {
        CategoryId = categoryId;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    
    public void UpdatePrices(decimal purchasePrice, decimal salesPrice)
    {
        DefaultPurchasePrice = purchasePrice;
        SalesPrice = salesPrice;
    }

    public void AddUnit(Guid unitId, decimal conversionRate, bool isBaseUnit, decimal price = 0)
    {
        _itemUnits.Add(new ItemUnit(Guid.NewGuid(), Id, unitId, conversionRate, isBaseUnit, price));
    }

    public void ClearUnits()
    {
        _itemUnits.Clear();
    }
}
