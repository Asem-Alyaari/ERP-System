using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.Reports.Queries.GetTrialBalance;

public class TrialBalanceSpecification : BaseSpecification<AccountBalance>
{
    public TrialBalanceSpecification(Guid fiscalPeriodId, Guid? costCenterId, Guid? currencyId)
        : base(x => x.FiscalPeriodId == fiscalPeriodId && 
                    (!costCenterId.HasValue || x.CostCenterId == costCenterId) && 
                    (!currencyId.HasValue || x.CurrencyId == currencyId))
    {
        AddInclude(x => x.Account!);
    }
}
