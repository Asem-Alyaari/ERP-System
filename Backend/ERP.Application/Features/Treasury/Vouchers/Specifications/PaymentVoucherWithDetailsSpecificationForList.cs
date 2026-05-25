using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Treasury.Vouchers.Specifications;

public class PaymentVoucherWithDetailsSpecificationForList : BaseSpecification<PaymentVoucher>
{
    public PaymentVoucherWithDetailsSpecificationForList() : base(x => true)
    {
        AddInclude(x => x.Vendor);
        AddInclude(x => x.Customer);
        AddInclude(x => x.SourceAccount);
        AddInclude(x => x.DestinationAccount);
        AddInclude(x => x.CostCenter);
    }
}
