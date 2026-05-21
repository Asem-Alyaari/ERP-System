using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Inventory.Units.Specifications;

public class ItemUnitsByUnitIdSpecification : BaseSpecification<ItemUnit>
{
    public ItemUnitsByUnitIdSpecification(Guid unitId) 
        : base(x => x.UnitId == unitId)
    {
    }
}
