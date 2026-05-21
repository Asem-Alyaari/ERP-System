using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Sales.Customers.Specifications;

public class CustomerFilterCountSpecification : BaseSpecification<Customer>
{
    public CustomerFilterCountSpecification(string? searchTerm) 
        : base(x => string.IsNullOrEmpty(searchTerm) || 
                    x.CustomerCode.Contains(searchTerm) || 
                    x.NameAr.Contains(searchTerm) || 
                    x.NameEn.Contains(searchTerm) || 
                    (x.TaxNumber != null && x.TaxNumber.Contains(searchTerm)) || 
                    (x.Phone != null && x.Phone.Contains(searchTerm)) || 
                    (x.Email != null && x.Email.Contains(searchTerm)))
    {
    }
}
