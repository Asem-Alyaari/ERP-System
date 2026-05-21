using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Units.Queries.GetUnitById;

public record GetUnitByIdQuery(Guid Id) : IRequest<UnitDto?>;

public class GetUnitByIdQueryHandler : IRequestHandler<GetUnitByIdQuery, UnitDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetUnitByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UnitDto?> Handle(GetUnitByIdQuery request, CancellationToken cancellationToken)
    {
        var unit = await _unitOfWork.Repository<ERP.Domain.Entities.Unit>().GetByIdAsync(request.Id);
        
        if (unit == null) return null;

        return new UnitDto
        {
            Id = unit.Id,
            NameAr = unit.NameAr,
            NameEn = unit.NameEn,
            ShortName = unit.ShortName
        };
    }
}
