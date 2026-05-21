using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.Currencies.Specifications;

public class CurrencyFilterCountSpecification : BaseSpecification<Currency>
{
    public CurrencyFilterCountSpecification(string? searchTerm) 
        : base(x => string.IsNullOrEmpty(searchTerm) || 
                    x.Code.Contains(searchTerm) || 
                    x.Name.Contains(searchTerm) || 
                    x.Symbol.Contains(searchTerm))
    {
    }
}
