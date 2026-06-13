using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Warehouses.Commands.Create;

public record CreateWarehouseCommand : IRequest<Guid>
{
    public string Code { get; init; } = string.Empty;
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? Location { get; init; }
}

public class CreateWarehouseCommandHandler : IRequestHandler<CreateWarehouseCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateWarehouseCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
    {
        var warehouse = new Warehouse(
            Guid.NewGuid(),
            request.Code,
            request.NameAr,
            request.NameEn,
            request.Location
        );

        _unitOfWork.Repository<Warehouse>().Add(warehouse);
        await _unitOfWork.Complete();

        return warehouse.Id;
    }
}
