using ERP.Domain.Entities;
using ERP.Domain.Specifications;
using System;

namespace ERP.Application.Features.Sales.Customers.Specifications;

public class ReceiptVouchersByCustomerIdSpecification : BaseSpecification<ReceiptVoucher>
{
    public ReceiptVouchersByCustomerIdSpecification(Guid customerId) 
        : base(x => x.CustomerId == customerId)
    {
    }
}
