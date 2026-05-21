using ERP.Application.Features.Accounting.JournalEntries.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.JournalEntries.Commands.Post;

public class PostJournalEntryCommandHandler : IRequestHandler<PostJournalEntryCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public PostJournalEntryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
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
        if (entry.Status != JournalEntryStatus.Draft)
            throw new BusinessException($"لا يمكن ترحيل القيد لأنه بحالة: {entry.Status}");

        // ج- التحقق من الفترة المالية (هل ما زالت مفتوحة؟)
        if (entry.FiscalPeriod == null)
            throw new BusinessException("الفترة المالية المرتبطة بالقيد غير موجودة.");

        if (entry.FiscalPeriod.IsClosed)
            throw new BusinessException("لا يمكن ترحيل القيد لأن الفترة المالية مغلقة.");

        // د- التحقق من توازن القيد (خطوة أمان إضافية)
        var totalDebit = entry.Lines.Sum(x => x.Debit);
        var totalCredit = entry.Lines.Sum(x => x.Credit);

        if (totalDebit != totalCredit)
            throw new BusinessException($"القيد غير متوازن مالياً. إجمالي المدين: {totalDebit}، إجمالي الدائن: {totalCredit}");

        // البدء بالترحيل داخل Transaction
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 2. تحديث حالة القيد وبيانات المراجعة
            entry.Post(request.PostedBy);
            _unitOfWork.Repository<JournalEntryMaster>().Update(entry);

            // 3. تحديث الأرصدة التراكمية لكل سطر في القيد
            foreach (var line in entry.Lines)
            {
                var balanceSpec = new AccountBalances.Specifications.AccountBalanceFilterSpecification(
                    entry.FiscalPeriodId, 
                    line.AccountId, 
                    line.CostCenterId, 
                    line.CurrencyId
                );

                var balance = await _unitOfWork.Repository<AccountBalance>().GetEntityWithSpec(balanceSpec);

                if (balance == null)
                {
                    // إنشاء سجل رصيد جديد إذا لم يكن موجوداً
                    balance = new AccountBalance(
                        Guid.NewGuid(),
                        entry.FiscalPeriodId,
                        line.AccountId,
                        line.CurrencyId,
                        line.CostCenterId
                    );
                    
                    balance.AddTransaction(line.Debit, line.Credit);
                    _unitOfWork.Repository<AccountBalance>().Add(balance);
                }
                else
                {
                    // تحديث السجل الموجود
                    balance.AddTransaction(line.Debit, line.Credit);
                    _unitOfWork.Repository<AccountBalance>().Update(balance);
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
