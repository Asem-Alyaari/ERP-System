using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Inventory.Units.Specifications;

public class UnitFilterSpecification : BaseSpecification<ERP.Domain.Entities.Unit>
{
    public UnitFilterSpecification(string? searchTerm, int skip, int take) 
        : base(x => string.IsNullOrEmpty(searchTerm) || x.NameAr.Contains(searchTerm) || x.NameEn.Contains(searchTerm) || (x.ShortName != null && x.ShortName.Contains(searchTerm)))
    {
        ApplyPaging(skip, take);
    }
}
