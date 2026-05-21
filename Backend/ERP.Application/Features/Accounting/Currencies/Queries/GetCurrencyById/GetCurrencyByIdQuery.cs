using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.Currencies.Queries.GetCurrencyById;

public record GetCurrencyByIdQuery(Guid Id) : IRequest<CurrencyDto?>;

public class GetCurrencyByIdQueryHandler : IRequestHandler<GetCurrencyByIdQuery, CurrencyDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCurrencyByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CurrencyDto?> Handle(GetCurrencyByIdQuery request, CancellationToken cancellationToken)
    {
        var currency = await _unitOfWork.Repository<Currency>().GetByIdAsync(request.Id);

        if (currency == null) return null;

        return new CurrencyDto
        {
            Id = currency.Id,
            Code = currency.Code,
            Name = currency.Name,
            Symbol = currency.Symbol,
            IsLocal = currency.IsLocal
        };
    }
}
