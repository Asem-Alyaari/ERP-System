using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Accounting.CurrencyExchangeRates.Specifications;
using MediatR;

namespace ERP.Application.Features.Accounting.CurrencyExchangeRates.Queries.GetCurrencyExchangeRates;

public record GetCurrencyExchangeRatesQuery(Guid? CurrencyId = null) : IRequest<List<CurrencyExchangeRateDto>>;

public class GetCurrencyExchangeRatesQueryHandler : IRequestHandler<GetCurrencyExchangeRatesQuery, List<CurrencyExchangeRateDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCurrencyExchangeRatesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CurrencyExchangeRateDto>> Handle(GetCurrencyExchangeRatesQuery request, CancellationToken cancellationToken)
    {
        var spec = new CurrencyExchangeRatesListSpecification(request.CurrencyId);
        var exchangeRates = await _unitOfWork.Repository<CurrencyExchangeRate>().ListAsync(spec);

        return exchangeRates.Select(x => new CurrencyExchangeRateDto
        {
            Id = x.Id,
            CurrencyId = x.CurrencyId,
            CurrencyCode = x.Currency?.Code ?? string.Empty,
            CurrencyName = x.Currency?.Name ?? string.Empty,
            Rate = x.Rate,
            EffectiveDate = x.EffectiveDate
        }).ToList();
    }
}
