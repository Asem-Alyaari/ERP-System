using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.Currencies.Specifications;

public class JournalEntryLinesByCurrencyIdSpecification : BaseSpecification<JournalEntryLine>
{
    public JournalEntryLinesByCurrencyIdSpecification(Guid currencyId) : base(x => x.CurrencyId == currencyId)
    {
    }
}
