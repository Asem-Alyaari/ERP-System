using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.CurrencyExchangeRates.Specifications;

public class CurrencyExchangeRateFilterSpecification : BaseSpecification<CurrencyExchangeRate>
{
    public CurrencyExchangeRateFilterSpecification(Guid? currencyId, string? searchTerm, int skip, int take)
        : base(x => (!currencyId.HasValue || x.CurrencyId == currencyId.Value) &&
                    (string.IsNullOrEmpty(searchTerm) || 
                     x.Currency!.Code.Contains(searchTerm) || 
                     x.Currency.Name.Contains(searchTerm)))
    {
        AddInclude(x => x.Currency!);
        ApplyOrderByDescending(x => x.EffectiveDate);
        ApplyPaging(skip, take);
    }
}
