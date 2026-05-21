using ERP.Domain.Entities;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using ERP.Application.Features.Purchasing.Vendors.Specifications;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Purchasing.Vendors.Commands.Delete;

public record DeleteVendorCommand(Guid Id) : IRequest<MediatR.Unit>;

public class DeleteVendorCommandHandler : IRequestHandler<DeleteVendorCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVendorCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(DeleteVendorCommand request, CancellationToken cancellationToken)
    {
        var vendor = await _unitOfWork.Repository<Vendor>().GetByIdAsync(request.Id);

        if (vendor == null)
        {
            throw new BusinessException("المورد المطلوب غير موجود.");
        }

        // Check if there are any invoices linked to this vendor
        var invoicesCount = await _unitOfWork.Repository<PurchaseInvoiceMaster>().CountAsync(new PurchaseInvoicesByVendorIdSpecification(request.Id));
        if (invoicesCount > 0)
        {
            throw new BusinessException("لا يمكن حذف المورد لارتباطه بفواتير مشتريات.");
        }

        // Check if there are any payment vouchers linked to this vendor
        var vouchersCount = await _unitOfWork.Repository<PaymentVoucher>().CountAsync(new PaymentVouchersByVendorIdSpecification(request.Id));
        if (vouchersCount > 0)
        {
            throw new BusinessException("لا يمكن حذف المورد لارتباطه بسندات صرف.");
        }

        _unitOfWork.Repository<Vendor>().Delete(vendor);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
