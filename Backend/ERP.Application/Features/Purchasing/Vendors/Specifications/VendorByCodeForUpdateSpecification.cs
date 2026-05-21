using ERP.Domain.Entities;
using ERP.Domain.Specifications;
using System;

namespace ERP.Application.Features.Purchasing.Vendors.Specifications;

public class VendorByCodeForUpdateSpecification : BaseSpecification<Vendor>
{
    public VendorByCodeForUpdateSpecification(string code, Guid excludeId) 
        : base(x => x.VendorCode == code && x.Id != excludeId)
    {
    }
}
