using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Warehouses.Commands.Update;

public record UpdateWarehouseCommand : IRequest<MediatR.Unit>
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Location { get; init; }
    public bool IsActive { get; init; }
}

public class UpdateWarehouseCommandHandler : IRequestHandler<UpdateWarehouseCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateWarehouseCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = await _unitOfWork.Repository<Warehouse>().GetByIdAsync(request.Id);

        if (warehouse == null)
        {
            throw new Exception($"Warehouse with ID {request.Id} not found");
        }

        warehouse.Update(request.Code, request.NameAr, request.NameEn, request.Location);

        if (request.IsActive)
            warehouse.Activate();
        else
            warehouse.Deactivate();

        _unitOfWork.Repository<Warehouse>().Update(warehouse);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
