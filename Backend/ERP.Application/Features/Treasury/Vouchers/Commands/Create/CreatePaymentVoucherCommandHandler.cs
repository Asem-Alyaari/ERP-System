using ERP.Application.Features.Accounting.AccountBalances.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using MediatR;
using System;
using System.Linq;
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
        // 1. التحقق من الحساب المصدر (يجب أن يكون خزينة أو بنك)
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

        // 3. التحقق من مركز التكلفة إذا تم تحديده
        if (request.CostCenterId.HasValue && request.CostCenterId.Value != Guid.Empty)
        {
            var costCenter = await _unitOfWork.Repository<CostCenter>().GetByIdAsync(request.CostCenterId.Value);
            if (costCenter == null)
                throw new BusinessException("مركز التكلفة غير موجود.");
        }

        // 4. الحصول على الفترة المالية المفتوحة
        var fiscalPeriods = await _unitOfWork.Repository<FiscalPeriod>().ListAllAsync();
        var fiscalPeriod = fiscalPeriods.FirstOrDefault(fp => !fp.IsClosed);
        
        if (fiscalPeriod == null)
            throw new BusinessException("لا توجد فترة مالية مفتوحة.");

        // 5. الحصول على العملة المحلية
        var currencies = await _unitOfWork.Repository<Currency>().ListAllAsync();
        var localCurrency = currencies.FirstOrDefault(c => c.IsLocal);
        
        if (localCurrency == null)
            throw new BusinessException("العملة المحلية غير موجودة.");

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
                request.DestinationAccountId,
                request.CostCenterId
            );

            _unitOfWork.Repository<PaymentVoucher>().Add(voucher);
            await _unitOfWork.Complete();

            // 7. إنشاء القيد المحاسبي التلقائي
            // تحديد الحساب المدين (الوجهة)
            Guid debitAccountId;
            if (request.DestinationType == VoucherPartnerType.Account)
            {
                debitAccountId = request.DestinationAccountId!.Value;
            }
            else if (request.DestinationType == VoucherPartnerType.Vendor)
            {
                // للموردين، نستخدم حساب المورد المحدد إذا وجد، أو نبحث عن حساب ذمم الموردين
                if (request.DestinationAccountId.HasValue)
                {
                    debitAccountId = request.DestinationAccountId.Value;
                }
                else
                {
                    // البحث عن حساب المورد (ذمم الموردين)
                    var accounts = await _unitOfWork.Repository<Account>().ListAllAsync();
                    var vendorsPayable = accounts.FirstOrDefault(a => a.AccountCode.StartsWith("21")); // أي حساب يبدأ بـ 21 (ذمم)
                    if (vendorsPayable == null)
                        throw new BusinessException("حساب ذمم الموردين غير موجود في دليل الحسابات. يرجى إنشاء حساب يبدأ بـ 21.");
                    debitAccountId = vendorsPayable.Id;
                }
            }
            else
            {
                throw new BusinessException("نوع الوجهة غير مدعوم.");
            }

            var journalEntry = new JournalEntryMaster(
                Guid.NewGuid(),
                $"JE-{request.VoucherNumber}", // ربط رقم القيد برقم السند
                request.VoucherDate,
                $"سند صرف رقم {request.VoucherNumber}",
                fiscalPeriod.Id,
                request.CreatedBy
            );

            _unitOfWork.Repository<JournalEntryMaster>().Add(journalEntry);

            // سطر مدين (الوجهة)
            var debitLine = new JournalEntryLine(
                Guid.NewGuid(),
                journalEntry.Id,
                debitAccountId,
                debit: request.Amount,
                credit: 0,
                localCurrency.Id,
                exchangeRate: 1,
                costCenterId: request.CostCenterId,
                memo: request.Notes
            );

            // سطر دائن (المصدر)
            var creditLine = new JournalEntryLine(
                Guid.NewGuid(),
                journalEntry.Id,
                request.SourceAccountId,
                debit: 0,
                credit: request.Amount,
                localCurrency.Id,
                exchangeRate: 1,
                costCenterId: null, // الحساب المصدر لا يحتاج مركز تكلفة
                memo: request.Notes
            );

            _unitOfWork.Repository<JournalEntryLine>().Add(debitLine);
            _unitOfWork.Repository<JournalEntryLine>().Add(creditLine);

            // 8. ترحيل القيد المحاسبي تلقائياً
            journalEntry.Post(request.CreatedBy);

            // 9. تحديث الأرصدة التراكمية
            foreach (var line in new[] { debitLine, creditLine })
            {
                var balanceSpec = new AccountBalanceFilterSpecification(
                    fiscalPeriodId: fiscalPeriod.Id,
                    accountId: line.AccountId,
                    costCenterId: line.CostCenterId,
                    currencyId: localCurrency.Id
                );

                var balance = await _unitOfWork.Repository<AccountBalance>().GetEntityWithSpec(balanceSpec);

                if (balance == null)
                {
                    balance = new AccountBalance(
                        Guid.NewGuid(),
                        fiscalPeriod.Id,
                        line.AccountId,
                        localCurrency.Id,
                        line.CostCenterId
                    );
                    balance.AddTransaction(line.Debit, line.Credit);
                    _unitOfWork.Repository<AccountBalance>().Add(balance);
                }
                else
                {
                    balance.AddTransaction(line.Debit, line.Credit);
                    _unitOfWork.Repository<AccountBalance>().Update(balance);
                }
            }

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
