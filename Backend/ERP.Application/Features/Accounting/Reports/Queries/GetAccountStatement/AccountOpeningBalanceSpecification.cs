using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.Reports.Queries.GetAccountStatement;

public class AccountOpeningBalanceSpecification : BaseSpecification<JournalEntryLine>
{
    public AccountOpeningBalanceSpecification(Guid accountId, DateTime fromDate)
        : base(x => x.AccountId == accountId && x.JournalEntryMaster!.TransactionDate < fromDate)
    {
    }
}
