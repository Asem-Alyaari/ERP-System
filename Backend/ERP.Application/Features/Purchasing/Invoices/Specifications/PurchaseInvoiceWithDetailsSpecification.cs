using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Purchasing.Invoices.Specifications;

public class PurchaseInvoiceWithDetailsSpecification : BaseSpecification<PurchaseInvoiceMaster>
{
    public PurchaseInvoiceWithDetailsSpecification(Guid id) 
        : base(x => x.Id == id)
    {
        AddInclude(x => x.Lines);
        AddInclude(x => x.Vendor!);
    }
}
