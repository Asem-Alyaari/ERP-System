using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Inventory.StockGroups.Specifications;

public class SubGroupsByParentGroupIdSpecification : BaseSpecification<StockGroup>
{
    public SubGroupsByParentGroupIdSpecification(Guid parentGroupId) : base(x => x.ParentGroupId == parentGroupId)
    {
    }
}
