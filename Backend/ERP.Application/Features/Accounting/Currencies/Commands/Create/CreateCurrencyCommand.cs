using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Accounting.Currencies.Specifications;
using MediatR;

namespace ERP.Application.Features.Accounting.Currencies.Commands.Create;

public record CreateCurrencyCommand : IRequest<Guid>
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Symbol { get; init; } = string.Empty;
    public bool IsLocal { get; init; }
}

public class CreateCurrencyCommandHandler : IRequestHandler<CreateCurrencyCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCurrencyCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCurrencyCommand request, CancellationToken cancellationToken)
    {
        // Business rule: If isLocal is true, we should ensure no other currency is set as local
        if (request.IsLocal)
        {
            var existingLocal = await _unitOfWork.Repository<Currency>().CountAsync(
                new LocalCurrencySpecification()
            );
            if (existingLocal > 0)
            {
                throw new Exception("There is already a local currency defined.");
            }
        }


        var currency = new Currency(
            Guid.NewGuid(),
            request.Code,
            request.Name,
            request.Symbol,
            request.IsLocal
        );

        _unitOfWork.Repository<Currency>().Add(currency);
        await _unitOfWork.Complete();

        return currency.Id;
    }
}
