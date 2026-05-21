using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Units.Queries.GetAllUnits;

public record GetAllUnitsQuery : IRequest<List<UnitDto>>;

public class GetAllUnitsQueryHandler : IRequestHandler<GetAllUnitsQuery, List<UnitDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllUnitsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<UnitDto>> Handle(GetAllUnitsQuery request, CancellationToken cancellationToken)
    {
        var units = await _unitOfWork.Repository<ERP.Domain.Entities.Unit>().ListAllAsync();
        
        return units.Select(u => new UnitDto
        {
            Id = u.Id,
            NameAr = u.NameAr,
            NameEn = u.NameEn,
            ShortName = u.ShortName
        }).ToList();
    }
}
