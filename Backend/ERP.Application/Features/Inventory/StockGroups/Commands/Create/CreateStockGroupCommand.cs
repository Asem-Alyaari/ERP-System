using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.StockGroups.Commands.Create;

public record CreateStockGroupCommand : IRequest<Guid>
{
    public string GroupCode { get; init; } = string.Empty;
    public string GroupNameAr { get; init; } = string.Empty;
    public string GroupNameEn { get; init; } = string.Empty;
    public bool IsDetail { get; init; }
    public Guid? ParentGroupId { get; init; }
    public Guid? InventoryAccountId { get; init; }
    public Guid? SalesAccountId { get; init; }
    public Guid? CostOfGoodsSoldAccountId { get; init; }
}

public class CreateStockGroupCommandHandler : IRequestHandler<CreateStockGroupCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateStockGroupCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateStockGroupCommand request, CancellationToken cancellationToken)
    {
        var stockGroup = new StockGroup(
            Guid.NewGuid(),
            request.GroupCode,
            request.GroupNameAr,
            request.GroupNameEn,
            request.IsDetail,
            request.ParentGroupId,
            request.InventoryAccountId,
            request.SalesAccountId,
            request.CostOfGoodsSoldAccountId
        );

        _unitOfWork.Repository<StockGroup>().Add(stockGroup);
        await _unitOfWork.Complete();

        return stockGroup.Id;
    }
}
