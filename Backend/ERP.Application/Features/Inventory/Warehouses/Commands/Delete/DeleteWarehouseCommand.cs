using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Warehouses.Commands.Delete;

public record DeleteWarehouseCommand(Guid Id) : IRequest<MediatR.Unit>;

public class DeleteWarehouseCommandHandler : IRequestHandler<DeleteWarehouseCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteWarehouseCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _unitOfWork.Repository<Warehouse>().GetByIdAsync(request.Id);

        if (warehouse == null)
        {
            throw new Exception($"Warehouse with ID {request.Id} not found");
        }

        // Check if there are any inventory transactions linked to this warehouse
        var hasTransactions = warehouse.InventoryTransactionMasters.Any() || warehouse.TransferToMasters.Any();
        if (hasTransactions)
        {
            throw new Exception("Cannot delete warehouse with linked inventory transactions");
        }

        _unitOfWork.Repository<Warehouse>().Delete(warehouse);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
