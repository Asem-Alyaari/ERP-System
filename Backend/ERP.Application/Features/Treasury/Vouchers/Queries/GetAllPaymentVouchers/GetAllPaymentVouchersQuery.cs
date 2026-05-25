using ERP.Application.Features.Treasury.Vouchers.DTOs;
using ERP.Application.Features.Treasury.Vouchers.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Treasury.Vouchers.Queries.GetAllPaymentVouchers;

public record GetAllPaymentVouchersQuery : IRequest<List<PaymentVoucherListItemDto>>;

public class GetAllPaymentVouchersQueryHandler : IRequestHandler<GetAllPaymentVouchersQuery, List<PaymentVoucherListItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllPaymentVouchersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PaymentVoucherListItemDto>> Handle(GetAllPaymentVouchersQuery request, CancellationToken cancellationToken)
    {
        var spec = new PaymentVoucherWithDetailsSpecificationForList();
        var vouchers = await _unitOfWork.Repository<PaymentVoucher>().ListAsync(spec);

        return vouchers.Select(v => new PaymentVoucherListItemDto
        {
            Id = v.Id,
            VoucherNumber = v.VoucherNumber,
            VoucherDate = v.VoucherDate,
            PaymentMethod = v.PaymentMethod,
            SourceAccountId = v.SourceAccountId,
            SourceAccountCode = v.SourceAccount?.AccountCode,
            SourceAccountNameAr = v.SourceAccount?.AccountNameAr,
            DestinationType = v.DestinationType,
            VendorId = v.VendorId,
            VendorName = v.Vendor?.NameAr,
            CustomerId = v.CustomerId,
            CustomerName = v.Customer?.NameAr,
            DestinationAccountId = v.DestinationAccountId,
            DestinationAccountCode = v.DestinationAccount?.AccountCode,
            DestinationAccountNameAr = v.DestinationAccount?.AccountNameAr,
            Amount = v.Amount,
            Notes = v.Notes,
            Status = v.Status,
            CreatedBy = v.CreatedBy,
            CreatedAt = v.CreatedAt,
            PostedBy = v.PostedBy,
            PostedAt = v.PostedAt,
            CostCenterId = v.CostCenterId,
            CostCenterCode = v.CostCenter?.CostCenterCode,
            CostCenterNameAr = v.CostCenter?.CostCenterNameAr
        }).ToList();
    }
}
