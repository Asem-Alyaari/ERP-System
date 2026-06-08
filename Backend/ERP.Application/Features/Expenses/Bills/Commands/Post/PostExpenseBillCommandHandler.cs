using ERP.Application.Features.Expenses.Bills.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using ERP.Domain.Services;
using MediatR;

namespace ERP.Application.Features.Expenses.Bills.Commands.Post;

public class PostExpenseBillCommandHandler : IRequestHandler<PostExpenseBillCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountBalanceService _accountBalanceService;

    public PostExpenseBillCommandHandler(
        IUnitOfWork unitOfWork,
        IAccountBalanceService accountBalanceService)
    {
        _unitOfWork = unitOfWork;
        _accountBalanceService = accountBalanceService;
    }

    public async Task<bool> Handle(PostExpenseBillCommand request, CancellationToken cancellationToken)
    {
        var spec = new ExpenseBillWithDetailsSpecification(request.BillId);
        var expenseBill = await _unitOfWork.Repository<ExpenseBillMaster>().GetEntityWithSpec(spec);

        if (expenseBill == null)
            throw new BusinessException("فاتورة المصروفات غير موجودة.");

        if (expenseBill.Status != ExpenseBillStatus.Draft)
            throw new BusinessException($"لا يمكن ترحيل الفاتورة لأنها بحالة: {expenseBill.Status}");

        // التحقق من أن تاريخ الفاتورة لا ينتمي إلى فترة مالية مغلقة
        var allPeriods = await _unitOfWork.Repository<FiscalPeriod>().ListAllAsync();
        var closedPeriod = allPeriods
            .FirstOrDefault(p => p.IsClosed && 
                               expenseBill.TransactionDate >= p.StartDate && 
                               expenseBill.TransactionDate <= p.EndDate);

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
            $"JV-EB-{expenseBill.BillNumber}",
            expenseBill.TransactionDate,
            expenseBill.Notes ?? $"قيد ناتج عن فاتورة مصروفات رقم: {expenseBill.BillNumber}",
            fiscalPeriod.Id,
            request.UserId
        );

        journalEntry.Post(request.UserId);
        _unitOfWork.Repository<JournalEntryMaster>().Add(journalEntry);

        var journalLines = new List<JournalEntryLine>();

        // تحديد الحساب الدائن بناءً على طريقة الدفع
        Guid creditAccountId;
        if (expenseBill.PaymentMethod == ExpenseBillPaymentMethod.Cash)
        {
            // نقدي: دائن على حساب الخزينة/الصندوق المحدد
            if (expenseBill.PaymentAccountId == null)
                throw new BusinessException("حساب الدفع غير محدد للمصروفات النقدية.");
            creditAccountId = expenseBill.PaymentAccountId.Value;
        }
        else if (expenseBill.PaymentMethod == ExpenseBillPaymentMethod.Bank)
        {
            // بنكي: دائن على حساب البنك المحدد
            if (expenseBill.PaymentAccountId == null)
                throw new BusinessException("حساب الدفع غير محدد للمصروفات البنكية.");
            creditAccountId = expenseBill.PaymentAccountId.Value;
        }
        else if (expenseBill.PaymentMethod == ExpenseBillPaymentMethod.Credit)
        {
            // آجل: دائن على حساب المورد
            if (expenseBill.Vendor == null)
                throw new BusinessException("المورد غير محدد للمصروفات الآجلة.");
            creditAccountId = expenseBill.Vendor.AccountId;
        }
        else
        {
            throw new BusinessException($"طريقة الدفع غير مدعومة: {expenseBill.PaymentMethod}");
        }

        // إنشاء سطر دائن واحد (مجموع المصروفات)
        var creditLine = new JournalEntryLine(
            Guid.NewGuid(), journalEntry.Id, creditAccountId,
            0, expenseBill.NetAmount, localCurrency.Id, 1, null, journalEntry.Description
        );
        journalLines.Add(creditLine);

        // إنشاء أسطر مدينة لكل سطر مصروف
        foreach (var line in expenseBill.Lines)
        {
            var debitLine = new JournalEntryLine(
                Guid.NewGuid(), journalEntry.Id, line.AccountId,
                line.Amount, 0, localCurrency.Id, 1, line.CostCenterId, line.Notes ?? journalEntry.Description
            );
            journalLines.Add(debitLine);
        }

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

        // تحديث حالة الفاتورة
        expenseBill.Post(request.UserId);
        _unitOfWork.Repository<ExpenseBillMaster>().Update(expenseBill);

        // حفظ التغييرات (TransactionBehavior سيتولى إدارة Transaction)
        await _unitOfWork.Complete();

        return true;
    }
}
