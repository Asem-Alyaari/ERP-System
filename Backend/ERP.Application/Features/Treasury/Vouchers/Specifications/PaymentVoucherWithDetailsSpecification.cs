using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Treasury.Vouchers.Specifications;

public class PaymentVoucherWithDetailsSpecification : BaseSpecification<PaymentVoucher>
{
    public PaymentVoucherWithDetailsSpecification(Guid id) 
        : base(x => x.Id == id)
    {
        AddInclude(x => x.Vendor!);
        AddInclude(x => x.Customer!);
        AddInclude(x => x.SourceAccount!);
        AddInclude(x => x.DestinationAccount!);
        AddInclude(x => x.CostCenter!);
    }
}
