using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.Reports.Queries.GetAccountStatement;

public class AccountStatementSpecification : BaseSpecification<JournalEntryLine>
{
    public AccountStatementSpecification(Guid accountId, DateTime? fromDate, DateTime? toDate)
        : base(x => x.AccountId == accountId && 
                   (!fromDate.HasValue || x.JournalEntryMaster!.TransactionDate >= fromDate.Value) &&
                   (!toDate.HasValue || x.JournalEntryMaster!.TransactionDate <= toDate.Value))
    {
        AddInclude(x => x.JournalEntryMaster!);
        ApplyOrderBy(x => x.JournalEntryMaster!.TransactionDate);
    }
}
