using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Inventory.StockGroups;
using ERP.Application.Features.Inventory.StockGroups.Specifications;
using MediatR;

namespace ERP.Application.Features.Inventory.StockGroups.Queries.GetStockGroupsWithPagination;

public record GetStockGroupsWithPaginationQuery : IRequest<StockGroupsPagedResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
}

public class StockGroupsPagedResponse
{
    public List<StockGroupDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class GetStockGroupsWithPaginationQueryHandler : IRequestHandler<GetStockGroupsWithPaginationQuery, StockGroupsPagedResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetStockGroupsWithPaginationQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StockGroupsPagedResponse> Handle(GetStockGroupsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;

        var spec = new StockGroupFilterSpecification(request.SearchTerm, skip, request.PageSize);
        var countSpec = new StockGroupFilterCountSpecification(request.SearchTerm);

        var items = await _unitOfWork.Repository<StockGroup>().ListAsync(spec);
        var totalCount = await _unitOfWork.Repository<StockGroup>().CountAsync(countSpec);

        return new StockGroupsPagedResponse
        {
            Items = items.Select(stockGroup => new StockGroupDto
            {
                Id = stockGroup.Id,
                GroupCode = stockGroup.GroupCode,
                GroupNameAr = stockGroup.GroupNameAr,
                GroupNameEn = stockGroup.GroupNameEn,
                IsDetail = stockGroup.IsDetail,
                ParentGroupId = stockGroup.ParentGroupId,
                InventoryAccountId = stockGroup.InventoryAccountId,
                SalesAccountId = stockGroup.SalesAccountId,
                CostOfGoodsSoldAccountId = stockGroup.CostOfGoodsSoldAccountId
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
