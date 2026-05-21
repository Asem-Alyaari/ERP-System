using ERP.Domain.Entities;
using ERP.Domain.Specifications;
using System;

namespace ERP.Application.Features.Sales.Customers.Specifications;

public class SalesInvoicesByCustomerIdSpecification : BaseSpecification<SalesInvoiceMaster>
{
    public SalesInvoicesByCustomerIdSpecification(Guid customerId) 
        : base(x => x.CustomerId == customerId)
    {
    }
}
