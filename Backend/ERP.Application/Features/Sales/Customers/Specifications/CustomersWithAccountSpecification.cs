using ERP.Domain.Entities;
using ERP.Domain.Specifications;
using System;

namespace ERP.Application.Features.Sales.Customers.Specifications;

public class CustomersWithAccountSpecification : BaseSpecification<Customer>
{
    public CustomersWithAccountSpecification() : base(x => true)
    {
        AddInclude(x => x.Account!);
    }
    
    public CustomersWithAccountSpecification(Guid id) : base(x => x.Id == id)
    {
        AddInclude(x => x.Account!);
    }
}
