using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.Accounts.Specifications;

public class AccountsWithCurrencySpecification : BaseSpecification<Account>
{
    public AccountsWithCurrencySpecification() : base(x => true)
    {
        AddInclude(x => x.Currency!);
        ApplyOrderBy(x => x.AccountCode);
    }
}
