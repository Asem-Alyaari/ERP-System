using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.CurrencyExchangeRates.Specifications;

public class CurrencyExchangeRatesListSpecification : BaseSpecification<CurrencyExchangeRate>
{
    public CurrencyExchangeRatesListSpecification(Guid? currencyId)
        : base(x => !currencyId.HasValue || x.CurrencyId == currencyId.Value)
    {
        AddInclude(x => x.Currency!);
        ApplyOrderByDescending(x => x.EffectiveDate);
    }
}
