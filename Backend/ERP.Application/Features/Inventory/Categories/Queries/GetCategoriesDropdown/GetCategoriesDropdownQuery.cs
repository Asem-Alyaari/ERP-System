using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Categories.Queries.GetCategoriesDropdown;

/// <summary>
/// استعلام جلب جميع التصنيفات بصيغة مسطّحة (Flat Lookup)
/// لتغذية القوائم المنسدلة في شاشة إنشاء/تعديل الأصناف.
/// </summary>
public record GetCategoriesDropdownQuery : IRequest<List<CategoryDto>>;

public class GetCategoriesDropdownQueryHandler
    : IRequestHandler<GetCategoriesDropdownQuery, List<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoriesDropdownQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CategoryDto>> Handle(
        GetCategoriesDropdownQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await _unitOfWork.Repository<Category>().ListAllAsync();

        return categories
            .OrderBy(c => c.NameAr)
            .Select(c => new CategoryDto
            {
                Id     = c.Id,
                NameAr = c.NameAr,
                NameEn = c.NameEn
            })
            .ToList();
    }
}
