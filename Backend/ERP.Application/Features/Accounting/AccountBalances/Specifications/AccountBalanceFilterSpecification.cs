using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.AccountBalances.Specifications;

public class AccountBalanceFilterSpecification : BaseSpecification<AccountBalance>
{
    public AccountBalanceFilterSpecification(Guid fiscalPeriodId, Guid accountId, Guid? costCenterId, Guid currencyId)
        : base(x => x.FiscalPeriodId == fiscalPeriodId && 
                    x.AccountId == accountId && 
                    x.CostCenterId == costCenterId && 
                    x.CurrencyId == currencyId)
    {
    }
}
