using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Inventory.Units.Specifications;

public class UnitFilterCountSpecification : BaseSpecification<ERP.Domain.Entities.Unit>
{
    public UnitFilterCountSpecification(string? searchTerm) 
        : base(x => string.IsNullOrEmpty(searchTerm) || x.NameAr.Contains(searchTerm) || x.NameEn.Contains(searchTerm) || (x.ShortName != null && x.ShortName.Contains(searchTerm)))
    {
    }
}
