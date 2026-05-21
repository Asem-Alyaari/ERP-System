using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Accounting.CurrencyExchangeRates.Specifications;
using MediatR;

namespace ERP.Application.Features.Accounting.CurrencyExchangeRates.Queries.GetCurrencyExchangeRatesWithPagination;

public record GetCurrencyExchangeRatesWithPaginationQuery : IRequest<CurrencyExchangeRatesPagedResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public Guid? CurrencyId { get; init; }
    public string? SearchTerm { get; init; }
}

public class CurrencyExchangeRatesPagedResponse
{
    public List<CurrencyExchangeRateDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class GetCurrencyExchangeRatesWithPaginationQueryHandler : IRequestHandler<GetCurrencyExchangeRatesWithPaginationQuery, CurrencyExchangeRatesPagedResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCurrencyExchangeRatesWithPaginationQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CurrencyExchangeRatesPagedResponse> Handle(GetCurrencyExchangeRatesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;

        var spec = new CurrencyExchangeRateFilterSpecification(request.CurrencyId, request.SearchTerm, skip, request.PageSize);
        var countSpec = new CurrencyExchangeRateFilterCountSpecification(request.CurrencyId, request.SearchTerm);

        var items = await _unitOfWork.Repository<CurrencyExchangeRate>().ListAsync(spec);
        var totalCount = await _unitOfWork.Repository<CurrencyExchangeRate>().CountAsync(countSpec);

        return new CurrencyExchangeRatesPagedResponse
        {
            Items = items.Select(x => new CurrencyExchangeRateDto
            {
                Id = x.Id,
                CurrencyId = x.CurrencyId,
                CurrencyCode = x.Currency?.Code ?? string.Empty,
                CurrencyName = x.Currency?.Name ?? string.Empty,
                Rate = x.Rate,
                EffectiveDate = x.EffectiveDate
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
