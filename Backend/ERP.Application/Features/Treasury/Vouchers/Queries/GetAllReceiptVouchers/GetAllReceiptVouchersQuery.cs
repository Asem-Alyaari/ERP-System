using ERP.Application.Features.Treasury.Vouchers.DTOs;
using ERP.Application.Features.Treasury.Vouchers.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Treasury.Vouchers.Queries.GetAllReceiptVouchers;

public record GetAllReceiptVouchersQuery : IRequest<List<ReceiptVoucherListItemDto>>;

public class GetAllReceiptVouchersQueryHandler : IRequestHandler<GetAllReceiptVouchersQuery, List<ReceiptVoucherListItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAllReceiptVouchersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ReceiptVoucherListItemDto>> Handle(GetAllReceiptVouchersQuery request, CancellationToken cancellationToken)
    {
        var spec = new ReceiptVoucherWithDetailsSpecificationForList();
        var vouchers = await _unitOfWork.Repository<ReceiptVoucher>().ListAsync(spec);

        return vouchers.Select(v => new ReceiptVoucherListItemDto
        {
            Id = v.Id,
            VoucherNumber = v.VoucherNumber,
            VoucherDate = v.VoucherDate,
            PaymentMethod = v.PaymentMethod,
            DestinationAccountId = v.DestinationAccountId,
            DestinationAccountCode = v.DestinationAccount?.AccountCode,
            DestinationAccountNameAr = v.DestinationAccount?.AccountNameAr,
            SourceType = v.SourceType,
            VendorId = v.VendorId,
            VendorName = v.Vendor?.NameAr,
            CustomerId = v.CustomerId,
            CustomerName = v.Customer?.NameAr,
            SourceAccountId = v.SourceAccountId,
            SourceAccountCode = v.SourceAccount?.AccountCode,
            SourceAccountNameAr = v.SourceAccount?.AccountNameAr,
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
