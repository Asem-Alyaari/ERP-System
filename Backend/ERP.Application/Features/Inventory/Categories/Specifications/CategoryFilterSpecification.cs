using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Inventory.Categories.Specifications;

public class CategoryFilterSpecification : BaseSpecification<Category>
{
    public CategoryFilterSpecification(string? searchTerm, int skip, int take) 
        : base(x => string.IsNullOrEmpty(searchTerm) || x.NameAr.Contains(searchTerm) || x.NameEn.Contains(searchTerm))
    {
        ApplyPaging(skip, take);
    }
}
