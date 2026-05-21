using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Inventory.Batches.Specifications;

public class ItemBatchSpecification : BaseSpecification<ItemBatch>
{
    public ItemBatchSpecification(Guid itemId, string batchNumber)
        : base(x => x.ItemId == itemId && x.BatchNumber == batchNumber)
    {
    }
}
