using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Inventory.Categories.Specifications;
using MediatR;

namespace ERP.Application.Features.Inventory.Categories.Queries.GetCategoriesWithPagination;

public record GetCategoriesWithPaginationQuery : IRequest<CategoriesPagedResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
}

public class CategoriesPagedResponse
{
    public List<CategoryDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class GetCategoriesWithPaginationQueryHandler : IRequestHandler<GetCategoriesWithPaginationQuery, CategoriesPagedResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoriesWithPaginationQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoriesPagedResponse> Handle(GetCategoriesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;
        
        var spec = new CategoryFilterSpecification(request.SearchTerm, skip, request.PageSize);
        var countSpec = new CategoryFilterCountSpecification(request.SearchTerm);

        var items = await _unitOfWork.Repository<Category>().ListAsync(spec);
        var totalCount = await _unitOfWork.Repository<Category>().CountAsync(countSpec);

        return new CategoriesPagedResponse
        {
            Items = items.Select(c => new CategoryDto
            {
                Id = c.Id,
                NameAr = c.NameAr,
                NameEn = c.NameEn
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
