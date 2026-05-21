using ERP.Domain.Entities;
using ERP.Domain.Specifications;
using System;

namespace ERP.Application.Features.Purchasing.Vendors.Specifications;

public class VendorsWithAccountSpecification : BaseSpecification<Vendor>
{
    public VendorsWithAccountSpecification() : base(x => true)
    {
        AddInclude(x => x.Account!);
    }
    
    public VendorsWithAccountSpecification(Guid id) : base(x => x.Id == id)
    {
        AddInclude(x => x.Account!);
    }
}
