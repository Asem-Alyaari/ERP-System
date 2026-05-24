using System;
using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.LedgerReport.Specifications;

public class LedgerTransactionSpecification : BaseSpecification<JournalEntryLine>
{
    public LedgerTransactionSpecification(
        Guid accountId,
        Guid? costCenterId,
        DateTime? fromDate,
        DateTime? toDate,
        bool includeBeforeDate = false)
        : base(x => 
            x.AccountId == accountId &&
            (!costCenterId.HasValue || x.CostCenterId == costCenterId.Value) &&
            (includeBeforeDate 
                ? (!fromDate.HasValue || (x.JournalEntryMaster != null && x.JournalEntryMaster.TransactionDate < fromDate.Value))
                : ((!fromDate.HasValue || (x.JournalEntryMaster != null && x.JournalEntryMaster.TransactionDate >= fromDate.Value)) &&
                   (!toDate.HasValue || (x.JournalEntryMaster != null && x.JournalEntryMaster.TransactionDate <= toDate.Value))))
        )
    {
        AddInclude(x => x.JournalEntryMaster);
        AddInclude(x => x.CostCenter);
        ApplyOrderBy(x => x.JournalEntryMaster != null ? x.JournalEntryMaster.TransactionDate : DateTime.MinValue);
    }
}
