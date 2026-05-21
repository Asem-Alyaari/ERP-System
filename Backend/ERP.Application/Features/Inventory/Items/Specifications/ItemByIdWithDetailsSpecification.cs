using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Inventory.Items.Specifications;

public class ItemByIdWithDetailsSpecification : BaseSpecification<Item>
{
    public ItemByIdWithDetailsSpecification(Guid id) : base(x => x.Id == id)
    {
        AddInclude(x => x.StockGroup!);
        AddInclude(x => x.Category!);
        AddInclude(x => x.ItemUnits);
        AddIncludeString("ItemUnits.Unit");
    }
}
