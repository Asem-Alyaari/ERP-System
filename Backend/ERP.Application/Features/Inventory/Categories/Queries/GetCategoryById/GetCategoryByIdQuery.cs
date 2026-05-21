using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto?>;

public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, CategoryDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoryByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryDto?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _unitOfWork.Repository<Category>().GetByIdAsync(request.Id);
        
        if (category == null) return null;

        return new CategoryDto
        {
            Id = category.Id,
            NameAr = category.NameAr,
            NameEn = category.NameEn
        };
    }
}
