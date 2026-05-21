using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.CurrencyExchangeRates.Commands.Create;

public record CreateCurrencyExchangeRateCommand : IRequest<Guid>
{
    public Guid CurrencyId { get; init; }
    public decimal Rate { get; init; }
    public DateTime EffectiveDate { get; init; }
}

public class CreateCurrencyExchangeRateCommandHandler : IRequestHandler<CreateCurrencyExchangeRateCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCurrencyExchangeRateCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCurrencyExchangeRateCommand request, CancellationToken cancellationToken)
    {
        // Verify currency exists
        var currency = await _unitOfWork.Repository<Currency>().GetByIdAsync(request.CurrencyId);
        if (currency == null)
        {
            throw new Exception("Currency not found.");
        }

        // Business rule: Local currency exchange rate should always be 1
        if (currency.IsLocal && request.Rate != 1)
        {
            throw new Exception("Exchange rate for the local currency must be 1.");
        }

        if (request.Rate <= 0)
        {
            throw new Exception("Exchange rate must be greater than zero.");
        }

        var exchangeRate = new CurrencyExchangeRate(
            Guid.NewGuid(),
            request.CurrencyId,
            request.Rate,
            request.EffectiveDate
        );

        _unitOfWork.Repository<CurrencyExchangeRate>().Add(exchangeRate);
        await _unitOfWork.Complete();

        return exchangeRate.Id;
    }
}
