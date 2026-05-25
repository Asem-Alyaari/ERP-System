using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Expenses.Bills.Specifications;

public class ExpenseBillWithDetailsSpecification : BaseSpecification<ExpenseBillMaster>
{
    public ExpenseBillWithDetailsSpecification(Guid id)
        : base(x => x.Id == id)
    {
        AddInclude(x => x.Vendor);
        AddInclude(x => x.PaymentAccount);
        AddInclude(x => x.Lines);
        AddIncludeString("Lines.Account");
        AddIncludeString("Lines.CostCenter");
    }
}
