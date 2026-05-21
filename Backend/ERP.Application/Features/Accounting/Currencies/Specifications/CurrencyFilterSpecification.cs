using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.Currencies.Specifications;

public class CurrencyFilterSpecification : BaseSpecification<Currency>
{
    public CurrencyFilterSpecification(string? searchTerm, int skip, int take) 
        : base(x => string.IsNullOrEmpty(searchTerm) || 
                    x.Code.Contains(searchTerm) || 
                    x.Name.Contains(searchTerm) || 
                    x.Symbol.Contains(searchTerm))
    {
        ApplyPaging(skip, take);
    }
}
