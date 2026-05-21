using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Units.Commands.Update;

public record UpdateUnitCommand : IRequest<MediatR.Unit>
{
    public Guid Id { get; init; }
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? ShortName { get; init; }
}

public class UpdateUnitCommandHandler : IRequestHandler<UpdateUnitCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateUnitCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await _unitOfWork.Repository<ERP.Domain.Entities.Unit>().GetByIdAsync(request.Id);

        if (unit == null)
        {
            throw new Exception($"Unit with ID {request.Id} not found");
        }

        unit.Update(request.NameAr, request.NameEn, request.ShortName);

        _unitOfWork.Repository<ERP.Domain.Entities.Unit>().Update(unit);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
