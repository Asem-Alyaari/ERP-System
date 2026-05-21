using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Inventory.StockGroups.Specifications;

public class ItemsByStockGroupIdSpecification : BaseSpecification<Item>
{
    public ItemsByStockGroupIdSpecification(Guid stockGroupId) : base(x => x.StockGroupId == stockGroupId)
    {
    }
}
