using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.StockGroups.Commands.Update;

public record UpdateStockGroupCommand : IRequest<MediatR.Unit>
{
    public Guid Id { get; init; }
    public string GroupCode { get; init; } = string.Empty;
    public string GroupNameAr { get; init; } = string.Empty;
    public string GroupNameEn { get; init; } = string.Empty;
    public bool IsDetail { get; init; }
    public Guid? ParentGroupId { get; init; }
    public Guid? InventoryAccountId { get; init; }
    public Guid? SalesAccountId { get; init; }
    public Guid? CostOfGoodsSoldAccountId { get; init; }
}

public class UpdateStockGroupCommandHandler : IRequestHandler<UpdateStockGroupCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateStockGroupCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(UpdateStockGroupCommand request, CancellationToken cancellationToken)
    {
        var stockGroup = await _unitOfWork.Repository<StockGroup>().GetByIdAsync(request.Id);

        if (stockGroup == null)
        {
            throw new Exception($"StockGroup with ID {request.Id} not found");
        }

        stockGroup.Update(
            request.GroupCode,
            request.GroupNameAr,
            request.GroupNameEn,
            request.IsDetail,
            request.ParentGroupId,
            request.InventoryAccountId,
            request.SalesAccountId,
            request.CostOfGoodsSoldAccountId
        );

        _unitOfWork.Repository<StockGroup>().Update(stockGroup);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
