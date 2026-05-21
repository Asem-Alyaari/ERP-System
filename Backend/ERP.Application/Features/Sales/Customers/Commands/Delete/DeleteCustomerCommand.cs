using ERP.Domain.Entities;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using ERP.Application.Features.Sales.Customers.Specifications;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Sales.Customers.Commands.Delete;

public record DeleteCustomerCommand(Guid Id) : IRequest<MediatR.Unit>;

public class DeleteCustomerCommandHandler : IRequestHandler<DeleteCustomerCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCustomerCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
    {
        var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(request.Id);

        if (customer == null)
        {
            throw new BusinessException("العميل المطلوب غير موجود.");
        }

        // Check if there are any invoices linked to this customer
        var invoicesCount = await _unitOfWork.Repository<SalesInvoiceMaster>().CountAsync(new SalesInvoicesByCustomerIdSpecification(request.Id));
        if (invoicesCount > 0)
        {
            throw new BusinessException("لا يمكن حذف العميل لارتباطه بفواتير مبيعات.");
        }

        // Check if there are any receipt vouchers linked to this customer
        var vouchersCount = await _unitOfWork.Repository<ReceiptVoucher>().CountAsync(new ReceiptVouchersByCustomerIdSpecification(request.Id));
        if (vouchersCount > 0)
        {
            throw new BusinessException("لا يمكن حذف العميل لارتباطه بسندات قبض.");
        }

        _unitOfWork.Repository<Customer>().Delete(customer);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
