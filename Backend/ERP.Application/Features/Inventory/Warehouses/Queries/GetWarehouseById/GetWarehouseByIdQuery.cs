using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Warehouses.Queries.GetWarehouseById;

public record GetWarehouseByIdQuery(Guid Id) : IRequest<WarehouseDto?>;

public class GetWarehouseByIdQueryHandler : IRequestHandler<GetWarehouseByIdQuery, WarehouseDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetWarehouseByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<WarehouseDto?> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        var warehouse = await _unitOfWork.Repository<Warehouse>().GetByIdAsync(request.Id);
        
        if (warehouse == null) return null;

        return new WarehouseDto
        {
            Id = warehouse.Id,
            Code = warehouse.Code,
            NameAr = warehouse.NameAr,
            NameEn = warehouse.NameEn,
            Location = warehouse.Location,
            IsActive = warehouse.IsActive
        };
    }
}
