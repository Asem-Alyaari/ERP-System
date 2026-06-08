using ERP.Application.Features.Treasury.Vouchers.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using ERP.Domain.Services;
using MediatR;

namespace ERP.Application.Features.Treasury.Vouchers.Commands.Post;

public record PostReceiptVoucherCommand(Guid VoucherId, string UserId) : IRequest<bool>;

public class PostReceiptVoucherCommandHandler : IRequestHandler<PostReceiptVoucherCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountBalanceService _accountBalanceService;

    public PostReceiptVoucherCommandHandler(
        IUnitOfWork unitOfWork,
        IAccountBalanceService accountBalanceService)
    {
        _unitOfWork = unitOfWork;
        _accountBalanceService = accountBalanceService;
    }

    public async Task<bool> Handle(PostReceiptVoucherCommand request, CancellationToken cancellationToken)
    {
        var spec = new ReceiptVoucherWithDetailsSpecification(request.VoucherId);
        var voucher = await _unitOfWork.Repository<ReceiptVoucher>().GetEntityWithSpec(spec);

        if (voucher == null)
            throw new BusinessException("سند القبض غير موجود.");

        if (voucher.Status != VoucherStatus.Draft)
            throw new BusinessException($"لا يمكن ترحيل السند لأنه بحالة: {voucher.Status}");

        // التحقق من أن تاريخ السند لا ينتمي إلى فترة مالية مغلقة
        var allPeriods = await _unitOfWork.Repository<FiscalPeriod>().ListAllAsync();
        var closedPeriod = allPeriods
            .FirstOrDefault(p => p.IsClosed && 
                               voucher.VoucherDate >= p.StartDate && 
                               voucher.VoucherDate <= p.EndDate);

        if (closedPeriod != null)
            throw new BusinessException("لا يمكن إجراء عمليات على فترة مالية مغلقة.");

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
            $"JV-RV-{voucher.VoucherNumber}",
            voucher.VoucherDate,
            voucher.Notes ?? $"قيد ناتج عن سند قبض رقم: {voucher.VoucherNumber}",
            fiscalPeriod.Id,
            request.UserId
        );

        journalEntry.Post(request.UserId);
        _unitOfWork.Repository<JournalEntryMaster>().Add(journalEntry);

        var journalLines = new List<JournalEntryLine>();

        // 1. الطرف المدين: الصندوق أو البنك (المستلم)
        var debitLine = new JournalEntryLine(
            Guid.NewGuid(), journalEntry.Id, voucher.DestinationAccountId,
            voucher.Amount, 0, localCurrency.Id, 1, null, journalEntry.Description
        );
        journalLines.Add(debitLine);

        // 2. الطرف الدائن: المصدر (عميل أو مورد أو حساب مباشر)
        Guid creditAccountId;
        if (voucher.SourceType == 0)
        {
            throw new BusinessException("نوع المصدر غير محدد. يرجى حذف السند وإنشائه مرة أخرى مع تحديد نوع المصدر بشكل صحيح.");
        }
        else if (voucher.SourceType == VoucherPartnerType.Customer)
        {
            if (voucher.Customer == null) throw new BusinessException("يجب تحديد العميل في حال كان نوع المصدر عميل.");
            creditAccountId = voucher.Customer.AccountId;
        }
        else if (voucher.SourceType == VoucherPartnerType.Vendor)
        {
            if (voucher.Vendor == null) throw new BusinessException("يجب تحديد المورد في حال كان نوع المصدر مورد.");
            creditAccountId = voucher.Vendor.AccountId;
        }
        else if (voucher.SourceType == VoucherPartnerType.Account)
        {
            if (voucher.SourceAccountId == null) throw new BusinessException("يجب تحديد حساب المصدر عند اختيار نوع المصدر كحساب.");
            creditAccountId = voucher.SourceAccountId.Value;
        }
        else
        {
            throw new BusinessException($"نوع المصدر غير مدعوم: {voucher.SourceType}");
        }

        var creditLine = new JournalEntryLine(
            Guid.NewGuid(), journalEntry.Id, creditAccountId,
            0, voucher.Amount, localCurrency.Id, 1, voucher.CostCenterId, journalEntry.Description
        );
        journalLines.Add(creditLine);

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
        _unitOfWork.Repository<ReceiptVoucher>().Update(voucher);

        // حفظ التغييرات (TransactionBehavior سيتولى إدارة Transaction)
        await _unitOfWork.Complete();

        return true;
    }
}
