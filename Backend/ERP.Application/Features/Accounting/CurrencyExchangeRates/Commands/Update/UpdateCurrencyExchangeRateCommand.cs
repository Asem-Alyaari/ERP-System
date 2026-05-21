using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.CurrencyExchangeRates.Commands.Update;

public record UpdateCurrencyExchangeRateCommand : IRequest
{
    public Guid Id { get; init; }
    public Guid CurrencyId { get; init; }
    public decimal Rate { get; init; }
    public DateTime EffectiveDate { get; init; }
}

public class UpdateCurrencyExchangeRateCommandHandler : IRequestHandler<UpdateCurrencyExchangeRateCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateCurrencyExchangeRateCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateCurrencyExchangeRateCommand request, CancellationToken cancellationToken)
    {
        var exchangeRate = await _unitOfWork.Repository<CurrencyExchangeRate>().GetByIdAsync(request.Id);
        if (exchangeRate == null)
        {
            throw new Exception("Currency exchange rate not found.");
        }

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

        exchangeRate.Update(request.CurrencyId, request.Rate, request.EffectiveDate);

        _unitOfWork.Repository<CurrencyExchangeRate>().Update(exchangeRate);
        await _unitOfWork.Complete();
    }
}
