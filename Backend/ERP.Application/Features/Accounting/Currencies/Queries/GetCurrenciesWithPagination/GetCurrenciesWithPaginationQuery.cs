using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Accounting.Currencies.Specifications;
using MediatR;

namespace ERP.Application.Features.Accounting.Currencies.Queries.GetCurrenciesWithPagination;

public record GetCurrenciesWithPaginationQuery : IRequest<CurrenciesPagedResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
}

public class CurrenciesPagedResponse
{
    public List<CurrencyDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class GetCurrenciesWithPaginationQueryHandler : IRequestHandler<GetCurrenciesWithPaginationQuery, CurrenciesPagedResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCurrenciesWithPaginationQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CurrenciesPagedResponse> Handle(GetCurrenciesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;

        var spec = new CurrencyFilterSpecification(request.SearchTerm, skip, request.PageSize);
        var countSpec = new CurrencyFilterCountSpecification(request.SearchTerm);

        var items = await _unitOfWork.Repository<Currency>().ListAsync(spec);
        var totalCount = await _unitOfWork.Repository<Currency>().CountAsync(countSpec);

        return new CurrenciesPagedResponse
        {
            Items = items.Select(currency => new CurrencyDto
            {
                Id = currency.Id,
                Code = currency.Code,
                Name = currency.Name,
                Symbol = currency.Symbol,
                IsLocal = currency.IsLocal
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
