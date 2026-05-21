using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Accounting.Currencies.Specifications;
using MediatR;

namespace ERP.Application.Features.Accounting.Currencies.Commands.Update;

public record UpdateCurrencyCommand : IRequest<MediatR.Unit>
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public bool IsLocal { get; init; }
}

public class UpdateCurrencyCommandHandler : IRequestHandler<UpdateCurrencyCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCurrencyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(UpdateCurrencyCommand request, CancellationToken cancellationToken)
    {
        var currency = await _unitOfWork.Repository<Currency>().GetByIdAsync(request.Id);

        if (currency == null)
        {
            throw new Exception($"Currency with ID {request.Id} not found");
        }

        // Business rule: If changing to local, verify no other currency is local
        if (request.IsLocal)
        {
            var existingLocal = await _unitOfWork.Repository<Currency>().CountAsync(
                new LocalCurrencySpecification(request.Id)
            );
            if (existingLocal > 0)
            {
                throw new Exception("There is already another currency set as local.");
            }
        }

        currency.Update(
            request.Code,
            request.Name,
            request.Symbol,
            request.IsLocal
        );

        _unitOfWork.Repository<Currency>().Update(currency);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
