using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Purchasing.Vendors.Specifications;

public class VendorByCodeSpecification : BaseSpecification<Vendor>
{
    public VendorByCodeSpecification(string code) : base(x => x.VendorCode == code)
    {
    }
}
