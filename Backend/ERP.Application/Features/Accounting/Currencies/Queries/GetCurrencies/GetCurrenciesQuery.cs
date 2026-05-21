using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.Currencies.Queries.GetCurrencies;

public record GetCurrenciesQuery : IRequest<List<CurrencyDto>>;

public class GetCurrenciesQueryHandler : IRequestHandler<GetCurrenciesQuery, List<CurrencyDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCurrenciesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CurrencyDto>> Handle(GetCurrenciesQuery request, CancellationToken cancellationToken)
    {
        var currencies = await _unitOfWork.Repository<Currency>().ListAllAsync();

        return currencies.Select(currency => new CurrencyDto
        {
            Id = currency.Id,
            Code = currency.Code,
            Name = currency.Name,
            Symbol = currency.Symbol,
            IsLocal = currency.IsLocal
        }).ToList();
    }
}
