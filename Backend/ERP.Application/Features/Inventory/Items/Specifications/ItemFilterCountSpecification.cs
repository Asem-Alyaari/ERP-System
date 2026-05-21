using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Inventory.Items.Specifications;

public class ItemFilterCountSpecification : BaseSpecification<Item>
{
    public ItemFilterCountSpecification(string? searchTerm) 
        : base(x => string.IsNullOrEmpty(searchTerm) || 
                    x.ItemCode.Contains(searchTerm) || 
                    x.ItemNameAr.Contains(searchTerm) || 
                    x.ItemNameEn.Contains(searchTerm) || 
                    (x.Sku != null && x.Sku.Contains(searchTerm)) || 
                    (x.Barcode != null && x.Barcode.Contains(searchTerm)))
    {
    }
}
