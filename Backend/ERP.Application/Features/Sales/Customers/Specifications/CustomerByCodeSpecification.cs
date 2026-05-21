using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Sales.Customers.Specifications;

public class CustomerByCodeSpecification : BaseSpecification<Customer>
{
    public CustomerByCodeSpecification(string code) 
        : base(x => x.CustomerCode == code)
    {
    }
}
