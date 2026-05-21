using ERP.Application.Features.Accounting.AccountBalances.Specifications;
using ERP.Application.Features.Inventory.Batches.Specifications;
using ERP.Application.Features.Inventory.Transactions.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Transactions.Commands.Approve;

public record ApproveStockOutCommand(Guid TransactionMasterId, string UserId) : IRequest<bool>;

public class ApproveStockOutCommandHandler : IRequestHandler<ApproveStockOutCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public ApproveStockOutCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(ApproveStockOutCommand request, CancellationToken cancellationToken)
    {
        // 1. جلب مستند الحركة مع التفاصيل
        var spec = new InventoryTransactionWithDetailsSpecification(request.TransactionMasterId);
        var transaction = await _unitOfWork.Repository<InventoryTransactionMaster>().GetEntityWithSpec(spec);

        if (transaction == null)
            throw new BusinessException("مستند الحركة المخزنية غير موجود.");

        if (transaction.Status != InventoryTransactionStatus.Draft)
            throw new BusinessException($"لا يمكن اعتماد المستند لأنه بحالة: {transaction.Status}");

        if (transaction.TransactionType != InventoryTransactionType.StockOut)
            throw new BusinessException("هذه العملية مخصصة لأذونات الصرف فقط.");

        // البدء بالمعاملة
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            decimal totalCogsAmount = 0;
            var accountingLinesSummary = new List<(Guid AccountId, decimal Debit, decimal Credit)>();

            // جلب الفترة المالية المفتوحة
            var fiscalPeriod = (await _unitOfWork.Repository<FiscalPeriod>().ListAllAsync())
                .FirstOrDefault(p => !p.IsClosed);

            if (fiscalPeriod == null)
                throw new BusinessException("لا توجد فترة مالية مفتوحة لتوليد القيد المحاسبي.");

            // --- أولاً: تحديث المخزون والتحقق من الكميات المتوفرة ---
            foreach (var line in transaction.Lines)
            {
                if (string.IsNullOrEmpty(line.BatchNumber))
                    throw new BusinessException($"يجب تحديد رقم الدفعة للصنف {line.Item?.ItemNameAr}");

                var batchSpec = new ItemBatchSpecification(line.ItemId, line.BatchNumber);
                var batch = await _unitOfWork.Repository<ItemBatch>().GetEntityWithSpec(batchSpec);

                if (batch == null)
                    throw new BusinessException($"الدفعة رقم ({line.BatchNumber}) غير موجودة للصنف {line.Item?.ItemNameAr}");

                if (batch.QuantityOnHand < line.BaseQuantity)
                    throw new BusinessException($"الكمية غير كافية في الدفعة ({line.BatchNumber}) للصنف {line.Item?.ItemNameAr}. المتوفر: {batch.QuantityOnHand}، المطلوب: {line.BaseQuantity}");

                // خصم الكمية من الدفعة
                batch.UpdateQuantity(-line.BaseQuantity);
                _unitOfWork.Repository<ItemBatch>().Update(batch);

                // --- ثانياً: احتساب تكلفة البضاعة المباعة (COGS) بناءً على سعر شراء الدفعة الفعلي ---
                var lineCogs = line.BaseQuantity * batch.PurchasePrice;
                
                if (line.Item?.StockGroup?.InventoryAccountId == null || line.Item?.StockGroup?.CostOfGoodsSoldAccountId == null)
                    throw new BusinessException($"مجموعة الأصناف {line.Item?.StockGroup?.GroupNameAr} غير مربوطة محاسبياً بشكل كامل.");

                // تجميع بيانات القيد (Debit COGS, Credit Inventory)
                accountingLinesSummary.Add((line.Item.StockGroup.CostOfGoodsSoldAccountId.Value, lineCogs, 0));
                accountingLinesSummary.Add((line.Item.StockGroup.InventoryAccountId.Value, 0, lineCogs));
            }

            // --- ثالثاً: توليد القيد المحاسبي للتكلفة ---
            var journalEntry = new JournalEntryMaster(
                Guid.NewGuid(),
                $"JV-SO-{transaction.DocumentNumber}",
                transaction.TransactionDate,
                $"قيد تكلفة آلي ناتج عن إذن صرف رقم: {transaction.DocumentNumber}",
                fiscalPeriod.Id,
                request.UserId
            );

            journalEntry.Post(request.UserId);
            _unitOfWork.Repository<JournalEntryMaster>().Add(journalEntry);

            // تجميع الأسطر حسب الحساب لتقليل حجم القيد
            var groupedAccountingLines = accountingLinesSummary
                .GroupBy(a => a.AccountId)
                .Select(g => new
                {
                    AccountId = g.Key,
                    TotalDebit = g.Sum(x => x.Debit),
                    TotalCredit = g.Sum(x => x.Credit)
                });

            foreach (var accLine in groupedAccountingLines)
            {
                var entryLine = new JournalEntryLine(
                    Guid.NewGuid(), journalEntry.Id, accLine.AccountId,
                    accLine.TotalDebit, accLine.TotalCredit, Guid.Empty, 1, null, journalEntry.Description
                );
                _unitOfWork.Repository<JournalEntryLine>().Add(entryLine);
                
                // تحديث الأرصدة التراكمية
                await UpdateAccountBalance(entryLine, fiscalPeriod.Id);
            }

            // --- رابعاً: تحديث حالة المستند المخزني ---
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
