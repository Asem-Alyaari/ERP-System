using ERP.Application.Features.Accounting.AccountBalances.Specifications;
using ERP.Application.Features.Inventory.Batches.Specifications;
using ERP.Application.Features.Inventory.Transactions.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Transactions.Commands.Approve;

public record ApproveStockInCommand(Guid TransactionMasterId, string UserId) : IRequest<bool>;

public class ApproveStockInCommandHandler : IRequestHandler<ApproveStockInCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApproveStockInCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ApproveStockInCommand request, CancellationToken cancellationToken)
    {
        // 1. جلب مستند الحركة مع التفاصيل
        var spec = new InventoryTransactionWithDetailsSpecification(request.TransactionMasterId);
        var transaction = await _unitOfWork.Repository<InventoryTransactionMaster>().GetEntityWithSpec(spec);

        if (transaction == null)
            throw new BusinessException("مستند الحركة المخزنية غير موجود.");

        if (transaction.Status != InventoryTransactionStatus.Draft)
            throw new BusinessException($"لا يمكن اعتماد المستند لأنه بحالة: {transaction.Status}");

        if (transaction.TransactionType != InventoryTransactionType.StockIn)
            throw new BusinessException("هذه العملية مخصصة لأذونات الإضافة فقط.");

        // البدء بالمعاملة
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // --- أولاً: تحديث المخزون والدفعات ---
            foreach (var line in transaction.Lines)
            {
                if (string.IsNullOrEmpty(line.BatchNumber))
                    throw new BusinessException($"يجب تحديد رقم الدفعة للصنف {line.Item?.ItemNameAr}");

                var batchSpec = new ItemBatchSpecification(line.ItemId, line.BatchNumber);
                var batch = await _unitOfWork.Repository<ItemBatch>().GetEntityWithSpec(batchSpec);

                if (batch != null)
                {
                    batch.UpdateQuantity(line.BaseQuantity);
                    _unitOfWork.Repository<ItemBatch>().Update(batch);
                }
                else
                {
                    // إنشاء دفعة جديدة
                    batch = new ItemBatch(
                        Guid.NewGuid(),
                        line.ItemId,
                        line.BatchNumber,
                        line.Price,
                        line.BaseQuantity
                    );
                    _unitOfWork.Repository<ItemBatch>().Add(batch);
                }
            }

            // --- ثانياً: توليد القيد المحاسبي التلقائي ---
            // سنفترض وجود فترة مالية فعالة (يمكن تطويرها لاحقاً لجلب الفترة الحالية)
            // للتبسيط، سنستخدم فترة افتراضية أو نطلبها من السياق
            // هنا سنبحث عن أقرب فترة مالية مفتوحة
            var fiscalPeriod = (await _unitOfWork.Repository<FiscalPeriod>().ListAllAsync())
                .FirstOrDefault(p => !p.IsClosed);

            if (fiscalPeriod == null)
                throw new BusinessException("لا توجد فترة مالية مفتوحة لتوليد القيد المحاسبي.");

            var journalEntry = new JournalEntryMaster(
                Guid.NewGuid(),
                $"JV-SI-{transaction.DocumentNumber}",
                transaction.TransactionDate,
                $"قيد آلي ناتج عن إذن إضافة رقم: {transaction.DocumentNumber}",
                fiscalPeriod.Id,
                request.UserId
            );

            journalEntry.Post(request.UserId); // ترحيل القيد فورياً
            _unitOfWork.Repository<JournalEntryMaster>().Add(journalEntry);

            // تجميع المبالغ حسب الحسابات لتوليد أسطر القيد
            var accountingLines = transaction.Lines
                .GroupBy(l => l.Item?.StockGroup)
                .Select(g => new
                {
                    StockGroup = g.Key,
                    TotalAmount = g.Sum(x => x.Total)
                });

            foreach (var accLine in accountingLines)
            {
                if (accLine.StockGroup?.InventoryAccountId == null || accLine.StockGroup?.CostOfGoodsSoldAccountId == null)
                    throw new BusinessException($"مجموعة الأصناف {accLine.StockGroup?.GroupNameAr} غير مربوطة محاسبياً بشكل كامل.");

                // الطرف المدين: المخزون
                var debitLine = new JournalEntryLine(
                    Guid.NewGuid(), journalEntry.Id, accLine.StockGroup.InventoryAccountId.Value,
                    accLine.TotalAmount, 0, Guid.Empty, 1, null, journalEntry.Description
                );
                _unitOfWork.Repository<JournalEntryLine>().Add(debitLine);
                await UpdateAccountBalance(debitLine, fiscalPeriod.Id);

                // الطرف الدائن: حساب وسيط/تكلفة (أو مشتريات)
                var creditLine = new JournalEntryLine(
                    Guid.NewGuid(), journalEntry.Id, accLine.StockGroup.CostOfGoodsSoldAccountId.Value,
                    0, accLine.TotalAmount, Guid.Empty, 1, null, journalEntry.Description
                );
                _unitOfWork.Repository<JournalEntryLine>().Add(creditLine);
                await UpdateAccountBalance(creditLine, fiscalPeriod.Id);
            }

            // --- ثالثاً: تحديث حالة المستند المخزني ---
            transaction.Approve(request.UserId);
            _unitOfWork.Repository<InventoryTransactionMaster>().Update(transaction);

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

    private async Task UpdateAccountBalance(JournalEntryLine line, Guid fiscalPeriodId)
    {
        // استخدام نفس منطق تحديث الأرصدة التراكمية
        var balanceSpec = new AccountBalanceFilterSpecification(
            fiscalPeriodId, line.AccountId, line.CostCenterId, line.CurrencyId
        );

        var balance = await _unitOfWork.Repository<AccountBalance>().GetEntityWithSpec(balanceSpec);

        if (balance == null)
        {
            balance = new AccountBalance(Guid.NewGuid(), fiscalPeriodId, line.AccountId, line.CurrencyId, line.CostCenterId);
            balance.AddTransaction(line.Debit, line.Credit);
            _unitOfWork.Repository<AccountBalance>().Add(balance);
        }
        else
        {
            balance.AddTransaction(line.Debit, line.Credit);
            _unitOfWork.Repository<AccountBalance>().Update(balance);
        }
    }
}
