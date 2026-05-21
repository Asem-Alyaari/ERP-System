using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Accounting.CurrencyExchangeRates.Specifications;
using MediatR;

namespace ERP.Application.Features.Accounting.CurrencyExchangeRates.Queries.GetCurrencyExchangeRateById;

public record GetCurrencyExchangeRateByIdQuery(Guid Id) : IRequest<CurrencyExchangeRateDto?>;

public class GetCurrencyExchangeRateByIdQueryHandler : IRequestHandler<GetCurrencyExchangeRateByIdQuery, CurrencyExchangeRateDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCurrencyExchangeRateByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CurrencyExchangeRateDto?> Handle(GetCurrencyExchangeRateByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new CurrencyExchangeRateWithCurrencySpecification(request.Id);
        var exchangeRate = await _unitOfWork.Repository<CurrencyExchangeRate>().GetEntityWithSpec(spec);

        if (exchangeRate == null) return null;

        return new CurrencyExchangeRateDto
        {
            Id = exchangeRate.Id,
            CurrencyId = exchangeRate.CurrencyId,
            CurrencyCode = exchangeRate.Currency?.Code ?? string.Empty,
            CurrencyName = exchangeRate.Currency?.Name ?? string.Empty,
            Rate = exchangeRate.Rate,
            EffectiveDate = exchangeRate.EffectiveDate
        };
    }
}
