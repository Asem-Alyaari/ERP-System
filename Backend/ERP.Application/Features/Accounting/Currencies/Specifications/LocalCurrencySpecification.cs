using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.Currencies.Specifications;

public class LocalCurrencySpecification : BaseSpecification<Currency>
{
    public LocalCurrencySpecification() : base(x => x.IsLocal)
    {
    }

    public LocalCurrencySpecification(Guid excludeId) : base(x => x.IsLocal && x.Id != excludeId)
    {
    }
}
