using ERP.Domain.Entities;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Treasury.Vouchers.Specifications;

public class ReceiptVoucherWithDetailsSpecification : BaseSpecification<ReceiptVoucher>
{
    public ReceiptVoucherWithDetailsSpecification(Guid id) 
        : base(x => x.Id == id)
    {
        AddInclude(x => x.Customer!);
    }
}
