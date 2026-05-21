using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Inventory.StockGroups;
using MediatR;

namespace ERP.Application.Features.Inventory.StockGroups.Queries.GetStockGroups;

public record GetStockGroupsQuery : IRequest<List<StockGroupDto>>;

public class GetStockGroupsQueryHandler : IRequestHandler<GetStockGroupsQuery, List<StockGroupDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetStockGroupsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<StockGroupDto>> Handle(GetStockGroupsQuery request, CancellationToken cancellationToken)
    {
        var stockGroups = await _unitOfWork.Repository<StockGroup>().ListAllAsync();

        return stockGroups.Select(stockGroup => new StockGroupDto
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
        }).ToList();
    }
}
