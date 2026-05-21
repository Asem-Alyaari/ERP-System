using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Inventory.Categories.Specifications;

public class CategoryFilterCountSpecification : BaseSpecification<Category>
{
    public CategoryFilterCountSpecification(string? searchTerm) 
        : base(x => string.IsNullOrEmpty(searchTerm) || x.NameAr.Contains(searchTerm) || x.NameEn.Contains(searchTerm))
    {
    }
}
