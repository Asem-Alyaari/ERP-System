using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Purchasing.Vendors.Specifications;

public class VendorFilterCountSpecification : BaseSpecification<Vendor>
{
    public VendorFilterCountSpecification(string? searchTerm) 
        : base(x => string.IsNullOrEmpty(searchTerm) || 
                    x.VendorCode.Contains(searchTerm) || 
                    x.NameAr.Contains(searchTerm) || 
                    x.NameEn.Contains(searchTerm) || 
                    (x.TaxNumber != null && x.TaxNumber.Contains(searchTerm)) || 
                    (x.Phone != null && x.Phone.Contains(searchTerm)) || 
                    (x.Email != null && x.Email.Contains(searchTerm)))
    {
    }
}
