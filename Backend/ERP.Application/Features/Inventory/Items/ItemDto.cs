namespace ERP.Application.Features.Inventory.Items;

public class ItemDto
{
    public Guid Id { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemNameAr { get; set; } = string.Empty;
    public string ItemNameEn { get; set; } = string.Empty;

    public Guid StockGroupId { get; set; }
    public string StockGroupNameAr { get; set; } = string.Empty;
    public string StockGroupNameEn { get; set; } = string.Empty;

    public Guid? CategoryId { get; set; }
    public string? CategoryNameAr { get; set; }
    public string? CategoryNameEn { get; set; }

    public string? Sku { get; set; }
    public string? Barcode { get; set; }

    public decimal DefaultPurchasePrice { get; set; }
    public decimal SalesPrice { get; set; }

    public bool IsActive { get; set; }

    public decimal ReorderLevel { get; set; }
    public decimal MinimumQuantity { get; set; }
    public decimal MaximumQuantity { get; set; }
    public Guid? SafetyStockUnitId { get; set; }

    public List<ItemUnitDto> ItemUnits { get; set; } = new();
}

public class ItemUnitDto
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string UnitNameAr { get; set; } = string.Empty;
    public string UnitNameEn { get; set; } = string.Empty;
    public decimal ConversionRate { get; set; }
    public bool IsBaseUnit { get; set; }
    public decimal Price { get; set; }
}
