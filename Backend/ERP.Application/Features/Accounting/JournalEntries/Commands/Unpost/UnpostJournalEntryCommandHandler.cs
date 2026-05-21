using ERP.Application.Features.Accounting.JournalEntries.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.JournalEntries.Commands.Unpost;

public class UnpostJournalEntryCommandHandler : IRequestHandler<UnpostJournalEntryCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public UnpostJournalEntryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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

        // البدء بالعملية داخل Transaction
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 2. إعادة حالة القيد إلى مسودة (Draft) وتصفير بيانات الترحيل
            entry.Unpost();
            _unitOfWork.Repository<JournalEntryMaster>().Update(entry);

            // 3. عكس الأثر المالي في جدول الأرصدة (SubtractTransaction)
            foreach (var line in entry.Lines)
            {
                var balanceSpec = new AccountBalances.Specifications.AccountBalanceFilterSpecification(
                    entry.FiscalPeriodId, 
                    line.AccountId, 
                    line.CostCenterId, 
                    line.CurrencyId
                );

                var balance = await _unitOfWork.Repository<AccountBalance>().GetEntityWithSpec(balanceSpec);

                if (balance != null)
                {
                    // طرح المبالغ من الرصيد التراكمي
                    balance.SubtractTransaction(line.Debit, line.Credit);
                    _unitOfWork.Repository<AccountBalance>().Update(balance);
                }
                else
                {
                    // حالة نادرة: إذا لم يوجد سجل رصيد لقيد مرحل (خلل في البيانات)
                    throw new BusinessException($"فشل إلغاء الترحيل: سجل الرصيد غير موجود للحساب ({line.AccountId})");
                }
            }

            await _unitOfWork.Complete();
            await _unitOfWork.CommitTransactionAsync();

            return true;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
