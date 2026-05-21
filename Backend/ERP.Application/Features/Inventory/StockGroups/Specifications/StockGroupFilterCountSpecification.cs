using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Inventory.StockGroups.Specifications;

public class StockGroupFilterCountSpecification : BaseSpecification<StockGroup>
{
    public StockGroupFilterCountSpecification(string? searchTerm) 
        : base(x => string.IsNullOrEmpty(searchTerm) || 
                    x.GroupCode.Contains(searchTerm) || 
                    x.GroupNameAr.Contains(searchTerm) || 
                    x.GroupNameEn.Contains(searchTerm))
    {
    }
}
