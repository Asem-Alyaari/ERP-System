using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Warehouses.Queries.GetWarehousesWithPagination;

public record GetWarehousesWithPaginationQuery : IRequest<WarehousesPagedResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
}

public class WarehousesPagedResponse
{
    public List<WarehouseDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class GetWarehousesWithPaginationQueryHandler : IRequestHandler<GetWarehousesWithPaginationQuery, WarehousesPagedResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetWarehousesWithPaginationQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<WarehousesPagedResponse> Handle(GetWarehousesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var warehouses = await _unitOfWork.Repository<Warehouse>().ListAllAsync();
        
        var filteredWarehouses = string.IsNullOrWhiteSpace(request.SearchTerm)
            ? warehouses
            : warehouses.Where(w => w.NameAr.Contains(request.SearchTerm) || w.NameEn.Contains(request.SearchTerm) || w.Code.Contains(request.SearchTerm));

        var totalCount = filteredWarehouses.Count();
        var items = filteredWarehouses
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(w => new WarehouseDto
            {
                Id = w.Id,
                Code = w.Code,
                NameAr = w.NameAr,
                NameEn = w.NameEn,
                Location = w.Location,
                IsActive = w.IsActive
            })
            .ToList();

        return new WarehousesPagedResponse
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
