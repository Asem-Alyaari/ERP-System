using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Items.Commands.Delete;

public record DeleteItemCommand(Guid Id) : IRequest<MediatR.Unit>;

public class DeleteItemCommandHandler : IRequestHandler<DeleteItemCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteItemCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.Repository<Item>().GetByIdAsync(request.Id);

        if (item == null)
        {
            throw new Exception($"Item with ID {request.Id} not found");
        }

        // Standard ERP safety check: You could check if there are transaction lines linked to this item
        // for now we will just delete the item from database.
        _unitOfWork.Repository<Item>().Delete(item);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
