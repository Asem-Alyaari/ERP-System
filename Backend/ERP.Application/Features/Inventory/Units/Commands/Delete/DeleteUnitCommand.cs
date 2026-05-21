using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Units.Commands.Delete;

public record DeleteUnitCommand(Guid Id) : IRequest<MediatR.Unit>;

public class DeleteUnitCommandHandler : IRequestHandler<DeleteUnitCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUnitCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(DeleteUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _unitOfWork.Repository<ERP.Domain.Entities.Unit>().GetByIdAsync(request.Id);

        if (unit == null)
        {
            throw new Exception($"Unit with ID {request.Id} not found");
        }

        // Check if there are any items linked to this unit
        var itemsCount = await _unitOfWork.Repository<ItemUnit>().CountAsync(new ERP.Application.Features.Inventory.Units.Specifications.ItemUnitsByUnitIdSpecification(request.Id));
        if (itemsCount > 0)
        {
            throw new Exception("Cannot delete unit with linked items");
        }

        _unitOfWork.Repository<ERP.Domain.Entities.Unit>().Delete(unit);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
