using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Inventory.Categories.Specifications;

public class ItemsByCategoryIdSpecification : BaseSpecification<Item>
{
    public ItemsByCategoryIdSpecification(Guid categoryId) 
        : base(x => x.CategoryId == categoryId)
    {
    }
}
