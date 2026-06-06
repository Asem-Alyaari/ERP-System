using ERP.Application.Features.Treasury.Vouchers.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using ERP.Domain.Services;
using MediatR;

namespace ERP.Application.Features.Treasury.Vouchers.Commands.Post;

public record PostPaymentVoucherCommand(Guid VoucherId, string UserId) : IRequest<bool>;

public class PostPaymentVoucherCommandHandler : IRequestHandler<PostPaymentVoucherCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountBalanceService _accountBalanceService;

    public PostPaymentVoucherCommandHandler(
        IUnitOfWork unitOfWork,
        IAccountBalanceService accountBalanceService)
    {
        _unitOfWork = unitOfWork;
        _accountBalanceService = accountBalanceService;
    }

    public async Task<bool> Handle(PostPaymentVoucherCommand request, CancellationToken cancellationToken)
    {
        var spec = new PaymentVoucherWithDetailsSpecification(request.VoucherId);
        var voucher = await _unitOfWork.Repository<PaymentVoucher>().GetEntityWithSpec(spec);

        if (voucher == null)
            throw new BusinessException("سند الصرف غير موجود.");

        if (voucher.Status != VoucherStatus.Draft)
            throw new BusinessException($"لا يمكن ترحيل السند لأنه بحالة: {voucher.Status}");

        var fiscalPeriod = (await _unitOfWork.Repository<FiscalPeriod>().ListAllAsync())
            .FirstOrDefault(p => !p.IsClosed);

        if (fiscalPeriod == null)
            throw new BusinessException("لا توجد فترة مالية مفتوحة لتوليد القيد المحاسبي.");

        // الحصول على العملة المحلية
        var currencies = await _unitOfWork.Repository<Currency>().ListAllAsync();
        var localCurrency = currencies.FirstOrDefault(c => c.IsLocal);

        if (localCurrency == null)
            throw new BusinessException("العملة المحلية غير موجودة.");

        // توليد القيد المحاسبي
        var journalEntry = new JournalEntryMaster(
            Guid.NewGuid(),
            $"JV-PV-{voucher.VoucherNumber}",
            voucher.VoucherDate,
            voucher.Notes ?? $"قيد ناتج عن سند صرف رقم: {voucher.VoucherNumber}",
            fiscalPeriod.Id,
            request.UserId
        );

        journalEntry.Post(request.UserId);
        _unitOfWork.Repository<JournalEntryMaster>().Add(journalEntry);

        var journalLines = new List<JournalEntryLine>();

        // 1. الطرف الدائن: الصندوق أو البنك (المصدر)
        var creditLine = new JournalEntryLine(
            Guid.NewGuid(), journalEntry.Id, voucher.SourceAccountId,
            0, voucher.Amount, localCurrency.Id, 1, null, journalEntry.Description
        );
        journalLines.Add(creditLine);

        // 2. الطرف المدين: الوجهة (عميل أو مورد أو حساب مباشر)
        Guid debitAccountId;
        if (voucher.DestinationType == 0)
        {
            throw new BusinessException("نوع الوجهة غير محدد. يرجى حذف السند وإنشائه مرة أخرى مع تحديد نوع الوجهة بشكل صحيح.");
        }
        else if (voucher.DestinationType == VoucherPartnerType.Customer)
        {
            if (voucher.Customer == null) throw new BusinessException("يجب تحديد العميل في حال كان نوع الوجهة عميل.");
            // استخدام حساب العميل إذا وجد، أو البحث عن حساب ذمم العملاء
            if (voucher.Customer.AccountId != Guid.Empty)
            {
                debitAccountId = voucher.Customer.AccountId;
            }
            else if (voucher.DestinationAccountId.HasValue)
            {
                debitAccountId = voucher.DestinationAccountId.Value;
            }
            else
            {
                // البحث عن حساب ذمم العملاء
                var accounts = await _unitOfWork.Repository<Account>().ListAllAsync();
                var customersReceivable = accounts.FirstOrDefault(a => a.AccountCode.StartsWith("11"));
                if (customersReceivable == null)
                    throw new BusinessException("حساب ذمم العملاء غير موجود في دليل الحسابات. يرجى إنشاء حساب يبدأ بـ 11.");
                debitAccountId = customersReceivable.Id;
            }
        }
        else if (voucher.DestinationType == VoucherPartnerType.Vendor)
        {
            if (voucher.Vendor == null) throw new BusinessException("يجب تحديد المورد في حال كان نوع الوجهة مورد.");
            // استخدام حساب المورد إذا وجد، أو البحث عن حساب ذمم الموردين
            if (voucher.Vendor.AccountId != Guid.Empty)
            {
                debitAccountId = voucher.Vendor.AccountId;
            }
            else if (voucher.DestinationAccountId.HasValue)
            {
                debitAccountId = voucher.DestinationAccountId.Value;
            }
            else
            {
                // البحث عن حساب ذمم الموردين
                var accounts = await _unitOfWork.Repository<Account>().ListAllAsync();
                var vendorsPayable = accounts.FirstOrDefault(a => a.AccountCode.StartsWith("21"));
                if (vendorsPayable == null)
                    throw new BusinessException("حساب ذمم الموردين غير موجود في دليل الحسابات. يرجى إنشاء حساب يبدأ بـ 21.");
                debitAccountId = vendorsPayable.Id;
            }
        }
        else if (voucher.DestinationType == VoucherPartnerType.Account)
        {
            if (voucher.DestinationAccountId == null) throw new BusinessException("يجب تحديد حساب الوجهة عند اختيار نوع الوجهة كحساب.");
            debitAccountId = voucher.DestinationAccountId.Value;
        }
        else
        {
            throw new BusinessException($"نوع الوجهة غير مدعوم: {voucher.DestinationType}");
        }

        var debitLine = new JournalEntryLine(
            Guid.NewGuid(), journalEntry.Id, debitAccountId,
            voucher.Amount, 0, localCurrency.Id, 1, voucher.CostCenterId, journalEntry.Description
        );
        journalLines.Add(debitLine);

        // إضافة جميع الأسطر
        foreach (var line in journalLines)
        {
            _unitOfWork.Repository<JournalEntryLine>().Add(line);
        }

        // تحديث الأرصدة التراكمية باستخدام الخدمة المركزية
        await _accountBalanceService.UpdateBalancesForLinesAsync(
            journalLines,
            fiscalPeriod.Id,
            cancellationToken);

        // تحديث حالة السند
        voucher.Post(request.UserId);
        _unitOfWork.Repository<PaymentVoucher>().Update(voucher);

        // حفظ التغييرات (TransactionBehavior سيتولى إدارة Transaction)
        await _unitOfWork.Complete();

        return true;
    }
}
