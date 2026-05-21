using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.Currencies.Specifications;

public class AccountsByCurrencyIdSpecification : BaseSpecification<Account>
{
    public AccountsByCurrencyIdSpecification(Guid currencyId) : base(x => x.CurrencyId == currencyId)
    {
    }
}
