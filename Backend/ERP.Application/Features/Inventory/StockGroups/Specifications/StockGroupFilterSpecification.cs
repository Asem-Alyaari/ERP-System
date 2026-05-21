using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Inventory.StockGroups.Specifications;

public class StockGroupFilterSpecification : BaseSpecification<StockGroup>
{
    public StockGroupFilterSpecification(string? searchTerm, int skip, int take) 
        : base(x => string.IsNullOrEmpty(searchTerm) || 
                    x.GroupCode.Contains(searchTerm) || 
                    x.GroupNameAr.Contains(searchTerm) || 
                    x.GroupNameEn.Contains(searchTerm))
    {
        ApplyPaging(skip, take);
    }
}
