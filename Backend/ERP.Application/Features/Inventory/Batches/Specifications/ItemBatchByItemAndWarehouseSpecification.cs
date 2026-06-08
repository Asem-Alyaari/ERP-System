using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Inventory.Batches.Specifications;

public class ItemBatchByItemAndWarehouseSpecification : BaseSpecification<ItemBatch>
{
    public ItemBatchByItemAndWarehouseSpecification(Guid itemId, Guid warehouseId)
        : base(x => x.ItemId == itemId && x.WarehouseId == warehouseId)
    {
        AddInclude(x => x.Item);
        AddInclude(x => x.Warehouse);
    }
}
