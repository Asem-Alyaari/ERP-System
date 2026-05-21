using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Purchasing.Vendors.Specifications;

public class PurchaseInvoicesByVendorIdSpecification : BaseSpecification<PurchaseInvoiceMaster>
{
    public PurchaseInvoicesByVendorIdSpecification(Guid vendorId) 
        : base(x => x.VendorId == vendorId)
    {
    }
}
