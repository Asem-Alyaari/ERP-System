using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Inventory.Units.Specifications;
using MediatR;

namespace ERP.Application.Features.Inventory.Units.Queries.GetUnitsWithPagination;

public record GetUnitsWithPaginationQuery : IRequest<UnitsPagedResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
}

public class UnitsPagedResponse
{
    public List<UnitDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class GetUnitsWithPaginationQueryHandler : IRequestHandler<GetUnitsWithPaginationQuery, UnitsPagedResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUnitsWithPaginationQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UnitsPagedResponse> Handle(GetUnitsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;
        
        var spec = new UnitFilterSpecification(request.SearchTerm, skip, request.PageSize);
        var countSpec = new UnitFilterCountSpecification(request.SearchTerm);

        var items = await _unitOfWork.Repository<ERP.Domain.Entities.Unit>().ListAsync(spec);
        var totalCount = await _unitOfWork.Repository<ERP.Domain.Entities.Unit>().CountAsync(countSpec);

        return new UnitsPagedResponse
        {
            Items = items.Select(u => new UnitDto
            {
                Id = u.Id,
                NameAr = u.NameAr,
                NameEn = u.NameEn,
                ShortName = u.ShortName
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
