using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Sales.Invoices.Specifications;

public class SalesInvoiceWithDetailsSpecification : BaseSpecification<SalesInvoiceMaster>
{
    public SalesInvoiceWithDetailsSpecification(Guid id) 
        : base(x => x.Id == id)
    {
        AddInclude(x => x.Lines.Select(l => l.Item!.StockGroup!));
        AddInclude(x => x.Customer!);
    }
}
