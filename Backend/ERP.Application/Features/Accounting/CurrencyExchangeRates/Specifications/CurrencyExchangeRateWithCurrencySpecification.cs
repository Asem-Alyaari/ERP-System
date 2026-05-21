using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.CurrencyExchangeRates.Specifications;

public class CurrencyExchangeRateWithCurrencySpecification : BaseSpecification<CurrencyExchangeRate>
{
    public CurrencyExchangeRateWithCurrencySpecification(Guid id)
        : base(x => x.Id == id)
    {
        AddInclude(x => x.Currency!);
    }
}
