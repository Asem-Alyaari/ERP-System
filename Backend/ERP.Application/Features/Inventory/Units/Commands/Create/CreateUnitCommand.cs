using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Units.Commands.Create;

public record CreateUnitCommand : IRequest<Guid>
{
    public string NameAr { get; init; } = string.Empty;
    public string NameEn { get; init; } = string.Empty;
    public string? ShortName { get; init; }
}

public class CreateUnitCommandHandler : IRequestHandler<CreateUnitCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateUnitCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = new ERP.Domain.Entities.Unit(
            Guid.NewGuid(),
            request.NameAr,
            request.NameEn,
            request.ShortName
        );

        _unitOfWork.Repository<ERP.Domain.Entities.Unit>().Add(unit);
        await _unitOfWork.Complete();

        return unit.Id;
    }
}
