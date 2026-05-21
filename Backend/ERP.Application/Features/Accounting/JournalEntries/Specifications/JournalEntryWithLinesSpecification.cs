using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.JournalEntries.Specifications;

public class JournalEntryWithLinesSpecification : BaseSpecification<JournalEntryMaster>
{
    public JournalEntryWithLinesSpecification(Guid id) 
        : base(x => x.Id == id)
    {
        AddInclude(x => x.Lines);
        AddInclude(x => x.FiscalPeriod!);
    }
}
