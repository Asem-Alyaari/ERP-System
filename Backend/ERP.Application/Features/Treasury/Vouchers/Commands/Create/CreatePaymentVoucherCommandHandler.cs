using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Treasury.Vouchers.Commands.Create;

public class CreatePaymentVoucherCommandHandler : IRequestHandler<CreatePaymentVoucherCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreatePaymentVoucherCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreatePaymentVoucherCommand request, CancellationToken cancellationToken)
    {
        // 1. التحقق من المبلغ
        if (request.Amount <= 0)
            throw new BusinessException("المبلغ يجب أن يكون أكبر من صفر.");

        // 2. التحقق من الحساب المصدر (يجب أن يكون خزينة أو بنك)
        var sourceAccount = await _unitOfWork.Repository<Account>().GetByIdAsync(request.SourceAccountId);
        if (sourceAccount == null)
            throw new BusinessException("الحساب المصدر غير موجود.");

        if (sourceAccount.AccountType != AccountType.Asset)
            throw new BusinessException("الحساب المصدر يجب أن يكون أصل (خزينة أو بنك).");

        // 2. التحقق من الوجهة
        if (request.DestinationType == VoucherPartnerType.Account)
        {
            if (!request.DestinationAccountId.HasValue)
                throw new BusinessException("يجب تحديد الحساب الوجهة عند اختيار نوع الوجهة كحساب.");

            var destAccount = await _unitOfWork.Repository<Account>().GetByIdAsync(request.DestinationAccountId.Value);
            if (destAccount == null)
                throw new BusinessException("الحساب الوجهة غير موجود.");

            // التحقق من مركز التكلفة إذا كان الحساب يتطلب ذلك
            if (destAccount.CostCenterStatus == CostCenterStatus.Required)
            {
                if (!request.CostCenterId.HasValue || request.CostCenterId.Value == Guid.Empty)
                    throw new BusinessException(
                        $"الحساب '{destAccount.AccountNameAr}' ({destAccount.AccountCode}) يتطلب مركز تكلفة إلزامياً. " +
                        $"يرجى تحديد مركز تكلفة.");
            }
        }
        else if (request.DestinationType == VoucherPartnerType.Vendor)
        {
            if (!request.VendorId.HasValue)
                throw new BusinessException("يجب تحديد المورد عند اختيار نوع الوجهة كمورد.");

            var vendor = await _unitOfWork.Repository<Vendor>().GetByIdAsync(request.VendorId.Value);
            if (vendor == null)
                throw new BusinessException("المورد غير موجود.");
        }
        else if (request.DestinationType == VoucherPartnerType.Customer)
        {
            if (!request.CustomerId.HasValue)
                throw new BusinessException("يجب تحديد العميل عند اختيار نوع الوجهة كعميل.");

            var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(request.CustomerId.Value);
            if (customer == null)
                throw new BusinessException("العميل غير موجود.");
        }

        // 3. التحقق من مركز التكلفة إذا تم تحديده
        if (request.CostCenterId.HasValue && request.CostCenterId.Value != Guid.Empty)
        {
            var costCenter = await _unitOfWork.Repository<CostCenter>().GetByIdAsync(request.CostCenterId.Value);
            if (costCenter == null)
                throw new BusinessException("مركز التكلفة غير موجود.");
        }

        // البدء بالمعاملة
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 6. إنشاء سند الصرف
            var voucher = new PaymentVoucher(
                Guid.NewGuid(),
                request.VoucherNumber,
                request.VoucherDate,
                request.PaymentMethod,
                request.SourceAccountId,
                request.DestinationType,
                request.Amount,
                request.CreatedBy,
                request.Notes,
                request.VendorId,
                request.CustomerId,
                request.DestinationAccountId,
                request.CostCenterId
            );

            _unitOfWork.Repository<PaymentVoucher>().Add(voucher);
            await _unitOfWork.Complete();
            await _unitOfWork.CommitTransactionAsync();

            return voucher.Id;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
