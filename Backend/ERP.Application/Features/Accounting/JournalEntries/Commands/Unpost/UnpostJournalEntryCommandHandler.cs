using ERP.Application.Features.Accounting.JournalEntries.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using ERP.Domain.Services;
using MediatR;

namespace ERP.Application.Features.Accounting.JournalEntries.Commands.Unpost;

public class UnpostJournalEntryCommandHandler : IRequestHandler<UnpostJournalEntryCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountBalanceService _accountBalanceService;

    public UnpostJournalEntryCommandHandler(
        IUnitOfWork unitOfWork,
        IAccountBalanceService accountBalanceService)
    {
        _unitOfWork = unitOfWork;
        _accountBalanceService = accountBalanceService;
    }

    public async Task<bool> Handle(UnpostJournalEntryCommand request, CancellationToken cancellationToken)
    {
        // 1. جلب القيد مع تفاصيله والفترة المالية
        var spec = new JournalEntryWithLinesSpecification(request.JournalEntryId);
        var entry = await _unitOfWork.Repository<JournalEntryMaster>().GetEntityWithSpec(spec);

        // أ- التحقق من وجود القيد
        if (entry == null)
            throw new BusinessException("قيد اليومية غير موجود.");

        // ب- التحقق من حالة القيد (يجب أن يكون مرحلاً حتماً)
        if (entry.Status != JournalEntryStatus.Posted)
            throw new BusinessException($"لا يمكن إلغاء ترحيل القيد لأنه بحالة: {entry.Status}");

        // ج- التحقق من الفترة المالية
        if (entry.FiscalPeriod == null)
            throw new BusinessException("الفترة المالية المرتبطة بالقيد غير موجودة.");

        if (entry.FiscalPeriod.IsClosed)
            throw new BusinessException("لا يمكن إلغاء ترحيل القيد لأن الفترة المالية مغلقة.");

        // 2. إعادة حالة القيد إلى مسودة (Draft) وتصفير بيانات الترحيل
        entry.Unpost();
        _unitOfWork.Repository<JournalEntryMaster>().Update(entry);

        // 3. عكس الأثر المالي في جدول الأرصدة باستخدام الخدمة المركزية
        await _accountBalanceService.ReverseBalancesForLinesAsync(
            entry.Lines,
            entry.FiscalPeriodId,
            cancellationToken);

        // 4. حفظ التغييرات (TransactionBehavior سيتولى إدارة Transaction)
        await _unitOfWork.Complete();

        return true;
    }
}
