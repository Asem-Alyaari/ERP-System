using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Inventory.StockGroups.Specifications;
using MediatR;

namespace ERP.Application.Features.Inventory.StockGroups.Commands.Delete;

public record DeleteStockGroupCommand(Guid Id) : IRequest<MediatR.Unit>;

public class DeleteStockGroupCommandHandler : IRequestHandler<DeleteStockGroupCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteStockGroupCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(DeleteStockGroupCommand request, CancellationToken cancellationToken)
    {
        var stockGroup = await _unitOfWork.Repository<StockGroup>().GetByIdAsync(request.Id);

        if (stockGroup == null)
        {
            throw new Exception($"StockGroup with ID {request.Id} not found");
        }

        // Check if there are any items linked to this group
        var itemsCount = await _unitOfWork.Repository<Item>().CountAsync(new ItemsByStockGroupIdSpecification(request.Id));
        if (itemsCount > 0)
        {
            throw new Exception("Cannot delete stock group with linked items");
        }

        // Check if there are any child subgroups linked to this group
        var childrenCount = await _unitOfWork.Repository<StockGroup>().CountAsync(new SubGroupsByParentGroupIdSpecification(request.Id));
        if (childrenCount > 0)
        {
            throw new Exception("Cannot delete stock group with child subgroups");
        }


        _unitOfWork.Repository<StockGroup>().Delete(stockGroup);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
