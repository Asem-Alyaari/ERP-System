using System;
using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Sales.Customers.Specifications;

public class CustomerByCodeForUpdateSpecification : BaseSpecification<Customer>
{
    public CustomerByCodeForUpdateSpecification(string code, Guid id) 
        : base(x => x.CustomerCode == code && x.Id != id)
    {
    }
}
