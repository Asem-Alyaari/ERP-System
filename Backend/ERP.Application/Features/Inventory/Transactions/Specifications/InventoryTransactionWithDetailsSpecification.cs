using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Inventory.Transactions.Specifications;

public class InventoryTransactionWithDetailsSpecification : BaseSpecification<InventoryTransactionMaster>
{
    public InventoryTransactionWithDetailsSpecification(Guid id) 
        : base(x => x.Id == id)
    {
        AddInclude(x => x.Lines.Select(l => l.Item!.StockGroup!));
    }
}
