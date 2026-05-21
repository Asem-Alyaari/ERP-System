using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Purchasing.Vendors.Specifications;

public class PaymentVouchersByVendorIdSpecification : BaseSpecification<PaymentVoucher>
{
    public PaymentVouchersByVendorIdSpecification(Guid vendorId) 
        : base(x => x.VendorId == vendorId)
    {
    }
}
