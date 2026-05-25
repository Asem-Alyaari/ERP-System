using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Treasury.Vouchers.Specifications;

public class ReceiptVoucherWithDetailsSpecificationForList : BaseSpecification<ReceiptVoucher>
{
    public ReceiptVoucherWithDetailsSpecificationForList() : base(x => true)
    {
        AddInclude(v => v.DestinationAccount);
        AddInclude(v => v.Customer);
        AddInclude(v => v.Vendor);
        AddInclude(v => v.SourceAccount);
        AddInclude(v => v.CostCenter);
    }
}
