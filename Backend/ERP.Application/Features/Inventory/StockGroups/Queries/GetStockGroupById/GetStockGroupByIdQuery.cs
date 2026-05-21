using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Inventory.StockGroups;
using MediatR;

namespace ERP.Application.Features.Inventory.StockGroups.Queries.GetStockGroupById;

public record GetStockGroupByIdQuery(Guid Id) : IRequest<StockGroupDto?>;

public class GetStockGroupByIdQueryHandler : IRequestHandler<GetStockGroupByIdQuery, StockGroupDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetStockGroupByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<StockGroupDto?> Handle(GetStockGroupByIdQuery request, CancellationToken cancellationToken)
    {
        var stockGroup = await _unitOfWork.Repository<StockGroup>().GetByIdAsync(request.Id);

        if (stockGroup == null) return null;

        return new StockGroupDto
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
        };
    }
}
