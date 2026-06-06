using ERP.Application.Features.Accounting.JournalEntries.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using ERP.Domain.Services;
using MediatR;

namespace ERP.Application.Features.Accounting.JournalEntries.Commands.Post;

public class PostJournalEntryCommandHandler : IRequestHandler<PostJournalEntryCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountBalanceService _accountBalanceService;

    public PostJournalEntryCommandHandler(
        IUnitOfWork unitOfWork,
        IAccountBalanceService accountBalanceService)
    {
        _unitOfWork = unitOfWork;
        _accountBalanceService = accountBalanceService;
    }

    public async Task<bool> Handle(PostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        // 1. جلب القيد مع تفاصيله والفترة المالية باستخدام Specification
        var spec = new JournalEntryWithLinesSpecification(request.JournalEntryId);
        var entry = await _unitOfWork.Repository<JournalEntryMaster>().GetEntityWithSpec(spec);

        // أ- التحقق من وجود القيد
        if (entry == null)
            throw new BusinessException("قيد اليومية غير موجود.");

        // ب- التحقق من حالة القيد (يجب أن يكون مسودة)
        if (entry.Status == JournalEntryStatus.Posted)
            throw new BusinessException("هذا القيد مرحل مسبقاً ولا يمكن ترحيله مرة أخرى");

        if (entry.Status == JournalEntryStatus.Cancelled)
            throw new BusinessException("لا يمكن ترحيل قيد ملغي.");

        // ج- التحقق من الفترة المالية (هل ما زالت مفتوحة؟)
        if (entry.FiscalPeriod == null)
            throw new BusinessException("الفترة المالية المرتبطة بالقيد غير موجودة.");

        if (entry.FiscalPeriod.IsClosed)
            throw new BusinessException("لا يمكن ترحيل القيد لأن الفترة المالية مغلقة.");

        // د- التحقق من توازن القيد (خطوة أمان إضافية)
        var totalDebit = entry.Lines.Sum(x => x.Debit);
        var totalCredit = entry.Lines.Sum(x => x.Credit);

        if (Math.Abs(totalDebit - totalCredit) > 0.01m)
            throw new BusinessException($"القيد غير متوازن مالياً. إجمالي المدين: {totalDebit}، إجمالي الدائن: {totalCredit}");

        // 2. تحديث حالة القيد وبيانات المراجعة
        entry.Post(request.PostedBy);
        _unitOfWork.Repository<JournalEntryMaster>().Update(entry);

        // 3. تحديث الأرصدة التراكمية باستخدام الخدمة المركزية
        await _accountBalanceService.UpdateBalancesForLinesAsync(
            entry.Lines,
            entry.FiscalPeriodId,
            cancellationToken);

        // 4. حفظ التغييرات (TransactionBehavior سيتولى إدارة Transaction)
        await _unitOfWork.Complete();

        return true;
    }
}
