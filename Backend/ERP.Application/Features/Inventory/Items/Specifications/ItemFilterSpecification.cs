using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Inventory.Items.Specifications;

public class ItemFilterSpecification : BaseSpecification<Item>
{
    public ItemFilterSpecification(string? searchTerm, int skip, int take) 
        : base(x => string.IsNullOrEmpty(searchTerm) || 
                    x.ItemCode.Contains(searchTerm) || 
                    x.ItemNameAr.Contains(searchTerm) || 
                    x.ItemNameEn.Contains(searchTerm) || 
                    (x.Sku != null && x.Sku.Contains(searchTerm)) || 
                    (x.Barcode != null && x.Barcode.Contains(searchTerm)))
    {
        AddInclude(x => x.StockGroup!);
        AddInclude(x => x.Category!);
        AddInclude(x => x.ItemUnits);
        AddIncludeString("ItemUnits.Unit");
        ApplyPaging(skip, take);
    }
}
