using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Treasury.Vouchers.Commands.Create;

public class CreateReceiptVoucherCommandHandler : IRequestHandler<CreateReceiptVoucherCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateReceiptVoucherCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateReceiptVoucherCommand request, CancellationToken cancellationToken)
    {
        // 1. التحقق من المبلغ
        if (request.Amount <= 0)
            throw new BusinessException("المبلغ يجب أن يكون أكبر من صفر.");

        // 2. التحقق من نوع المصدر
        if (request.SourceType == 0)
            throw new BusinessException("يجب تحديد نوع المصدر (عميل، مورد، أو حساب).");

        // 2. التحقق من الحساب الوجهة (يجب أن يكون خزينة أو بنك)
        var destinationAccount = await _unitOfWork.Repository<Account>().GetByIdAsync(request.DestinationAccountId);
        if (destinationAccount == null)
            throw new BusinessException("الحساب الوجهة غير موجود.");

        if (destinationAccount.AccountType != AccountType.Asset)
            throw new BusinessException("الحساب الوجهة يجب أن يكون أصل (خزينة أو بنك).");

        // 3. التحقق من المصدر
        if (request.SourceType == VoucherPartnerType.Account)
        {
            if (!request.SourceAccountId.HasValue)
                throw new BusinessException("يجب تحديد الحساب المصدر عند اختيار نوع المصدر كحساب.");

            var sourceAccount = await _unitOfWork.Repository<Account>().GetByIdAsync(request.SourceAccountId.Value);
            if (sourceAccount == null)
                throw new BusinessException("الحساب المصدر غير موجود.");

            // التحقق من مركز التكلفة إذا كان الحساب يتطلب ذلك
            if (sourceAccount.CostCenterStatus == CostCenterStatus.Required)
            {
                if (!request.CostCenterId.HasValue || request.CostCenterId.Value == Guid.Empty)
                    throw new BusinessException(
                        $"الحساب '{sourceAccount.AccountNameAr}' ({sourceAccount.AccountCode}) يتطلب مركز تكلفة إلزامياً. " +
                        $"يرجى تحديد مركز تكلفة.");
            }
        }
        else if (request.SourceType == VoucherPartnerType.Vendor)
        {
            if (!request.VendorId.HasValue)
                throw new BusinessException("يجب تحديد المورد عند اختيار نوع المصدر كمورد.");

            var vendor = await _unitOfWork.Repository<Vendor>().GetByIdAsync(request.VendorId.Value);
            if (vendor == null)
                throw new BusinessException("المورد غير موجود.");
        }
        else if (request.SourceType == VoucherPartnerType.Customer)
        {
            if (!request.CustomerId.HasValue)
                throw new BusinessException("يجب تحديد العميل عند اختيار نوع المصدر كعميل.");

            var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(request.CustomerId.Value);
            if (customer == null)
                throw new BusinessException("العميل غير موجود.");
        }

        // 4. التحقق من مركز التكلفة إذا تم تحديده
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
            // 5. إنشاء سند القبض
            var voucher = new ReceiptVoucher(
                Guid.NewGuid(),
                request.VoucherNumber,
                request.VoucherDate,
                request.PaymentMethod,
                request.DestinationAccountId,
                request.SourceType,
                request.Amount,
                request.CreatedBy,
                request.Notes,
                request.CustomerId,
                request.VendorId,
                request.SourceAccountId,
                request.CostCenterId
            );

            _unitOfWork.Repository<ReceiptVoucher>().Add(voucher);
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
