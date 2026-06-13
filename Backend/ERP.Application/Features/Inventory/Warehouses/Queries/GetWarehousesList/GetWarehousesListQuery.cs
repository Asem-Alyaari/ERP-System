using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Warehouses.Queries.GetWarehousesList;

/// <summary>
/// استعلام جلب جميع المستودعات بصيغة قائمة بسيطة
/// لتغذية القوائم المنسدلة والجداول
/// </summary>
public record GetWarehousesListQuery : IRequest<List<WarehouseDto>>;

public class GetWarehousesListQueryHandler : IRequestHandler<GetWarehousesListQuery, List<WarehouseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetWarehousesListQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<WarehouseDto>> Handle(GetWarehousesListQuery request, CancellationToken cancellationToken)
    {
        var warehouses = await _unitOfWork.Repository<Warehouse>().ListAllAsync();

        return warehouses
            .Select(w => new WarehouseDto
            {
                Id = w.Id,
                Code = w.Code,
                NameAr = w.NameAr,
                NameEn = w.NameEn,
                Location = w.Location,
                IsActive = w.IsActive
            })
            .ToList();
    }
}
