using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.CurrencyExchangeRates.Specifications;

public class CurrencyExchangeRateFilterCountSpecification : BaseSpecification<CurrencyExchangeRate>
{
    public CurrencyExchangeRateFilterCountSpecification(Guid? currencyId, string? searchTerm)
        : base(x => (!currencyId.HasValue || x.CurrencyId == currencyId.Value) &&
                    (string.IsNullOrEmpty(searchTerm) || 
                     x.Currency!.Code.Contains(searchTerm) || 
                     x.Currency.Name.Contains(searchTerm)))
    {
    }
}
