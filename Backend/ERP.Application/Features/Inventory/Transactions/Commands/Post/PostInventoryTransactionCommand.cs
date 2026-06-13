using ERP.Application.Features.Inventory.Batches.Specifications;
using ERP.Application.Features.Inventory.Transactions.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using ERP.Domain.Services;
using MediatR;

namespace ERP.Application.Features.Inventory.Transactions.Commands.Post;

public record PostInventoryTransactionCommand(Guid TransactionMasterId, string UserId) : IRequest<bool>;

public class PostInventoryTransactionCommandHandler : IRequestHandler<PostInventoryTransactionCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAccountBalanceService _accountBalanceService;

    public PostInventoryTransactionCommandHandler(
        IUnitOfWork unitOfWork,
        IAccountBalanceService accountBalanceService)
    {
        _unitOfWork = unitOfWork;
        _accountBalanceService = accountBalanceService;
    }

    public async Task<bool> Handle(PostInventoryTransactionCommand request, CancellationToken cancellationToken)
    {
        // 1. جلب مستند الحركة مع التفاصيل
        var spec = new InventoryTransactionWithDetailsSpecification(request.TransactionMasterId);
        var transaction = await _unitOfWork.Repository<InventoryTransactionMaster>().GetEntityWithSpec(spec);

        if (transaction == null)
            throw new BusinessException("مستند الحركة المخزنية غير موجود.");

        if (transaction.Status != InventoryTransactionStatus.Draft)
            throw new BusinessException($"لا يمكن ترحيل المستند لأنه بحالة: {transaction.Status}");

        if (transaction.IsPosted)
            throw new BusinessException("المستند تم ترحيله محاسبياً مسبقاً.");

        // جلب الفترة المالية المفتوحة
        var fiscalPeriod = (await _unitOfWork.Repository<FiscalPeriod>().ListAllAsync())
            .FirstOrDefault(p => !p.IsClosed);

        if (fiscalPeriod == null)
            throw new BusinessException("لا توجد فترة مالية مفتوحة لتوليد القيد المحاسبي.");

        // 2. معالجة الحركة حسب النوع
        switch (transaction.TransactionType)
        {
            case InventoryTransactionType.StockIn:
                await ProcessStockIn(transaction, fiscalPeriod.Id, request.UserId, cancellationToken);
                break;
            case InventoryTransactionType.StockOut:
                await ProcessStockOut(transaction, fiscalPeriod.Id, request.UserId, cancellationToken);
                break;
            case InventoryTransactionType.Transfer:
                await ProcessTransfer(transaction, fiscalPeriod.Id, request.UserId, cancellationToken);
                break;
            default:
                throw new BusinessException("نوع الحركة المخزنية غير مدعوم.");
        }

        // 3. تحديث حالة المستند وترحيله
        transaction.Approve(request.UserId);
        transaction.MarkAsPosted();
        _unitOfWork.Repository<InventoryTransactionMaster>().Update(transaction);

        await _unitOfWork.Complete();

        return true;
    }

    private async Task ProcessStockIn(
        InventoryTransactionMaster transaction,
        Guid fiscalPeriodId,
        string userId,
        CancellationToken cancellationToken)
    {
        var journalLines = new List<JournalEntryLine>();

        // Get default currency (first currency in system)
        var currencies = await _unitOfWork.Repository<Currency>().ListAllAsync();
        var defaultCurrency = currencies.FirstOrDefault();
        if (defaultCurrency == null)
            throw new BusinessException("لا توجد عملات في النظام.");

        foreach (var line in transaction.Lines)
        {
            if (line.Item == null)
                throw new BusinessException($"الصنف في السطر غير موجود.");

            if (line.Item.StockGroup == null)
                throw new BusinessException($"مجموعة الأصناف للصنف {line.Item.ItemNameAr} غير محددة.");

            // تحديث متوسط التكلفة (Moving Weighted Average)
            await UpdateAverageCost(line.Item, transaction.WarehouseId, line.BaseQuantity, line.Price);

            // تحديث الدفعات
            await UpdateBatchesForStockIn(line.ItemId, transaction.WarehouseId, line.BaseQuantity, line.Price, line.BatchNumber);

            // تجميع بيانات القيد المحاسبي
            if (line.Item.StockGroup.InventoryAccountId == null)
                throw new BusinessException($"مجموعة الأصناف {line.Item.StockGroup.GroupNameAr} غير مربوطة بحساب مخزون.");

            // Debit Inventory Account
            journalLines.Add(new JournalEntryLine(
                Guid.NewGuid(),
                Guid.Empty, // Will be set after creating journal entry
                line.Item.StockGroup.InventoryAccountId.Value,
                line.Total,
                0,
                defaultCurrency.Id,
                1,
                null,
                $"إضافة مخزون - {line.Item.ItemNameAr}"
            ));

            // Credit: Use Expense Account if available, otherwise use COGS
            var creditAccountId = line.Item.StockGroup.ExpenseAccountId ?? line.Item.StockGroup.CostOfGoodsSoldAccountId;
            if (creditAccountId == null)
                throw new BusinessException($"مجموعة الأصناف {line.Item.StockGroup.GroupNameAr} غير مربوطة بحساب مصاريف أو تكلفة.");

            journalLines.Add(new JournalEntryLine(
                Guid.NewGuid(),
                Guid.Empty,
                creditAccountId.Value,
                0,
                line.Total,
                defaultCurrency.Id,
                1,
                null,
                $"إضافة مخزون - {line.Item.ItemNameAr}"
            ));
        }

        // تجميع الأسطر حسب الحساب
        var groupedLines = journalLines
            .GroupBy(l => l.AccountId)
            .Select(g => new JournalEntryLine(
                Guid.NewGuid(),
                Guid.Empty,
                g.Key,
                g.Sum(x => x.Debit),
                g.Sum(x => x.Credit),
                defaultCurrency.Id,
                1,
                null,
                $"إذن إضافة رقم: {transaction.DocumentNumber}"
            ))
            .ToList();

        // إنشاء القيد المحاسبي
        await CreateAndPostJournalEntry(
            transaction,
            fiscalPeriodId,
            userId,
            groupedLines,
            cancellationToken);
    }

    private async Task ProcessStockOut(
        InventoryTransactionMaster transaction,
        Guid fiscalPeriodId,
        string userId,
        CancellationToken cancellationToken)
    {
        var journalLines = new List<JournalEntryLine>();

        // Get default currency
        var currencies = await _unitOfWork.Repository<Currency>().ListAllAsync();
        var defaultCurrency = currencies.FirstOrDefault();
        if (defaultCurrency == null)
            throw new BusinessException("لا توجد عملات في النظام.");

        foreach (var line in transaction.Lines)
        {
            if (line.Item == null)
                throw new BusinessException($"الصنف في السطر غير موجود.");

            if (line.Item.StockGroup == null)
                throw new BusinessException($"مجموعة الأصناف للصنف {line.Item.ItemNameAr} غير محددة.");

            // التحقق من الكمية المتوفرة
            await ValidateStockAvailability(line.Item.Id, transaction.WarehouseId, line.BaseQuantity);

            // تحديث الدفعات
            await UpdateBatchesForStockOut(line.ItemId, transaction.WarehouseId, line.BaseQuantity, line.BatchNumber);

            // حساب التكلفة باستخدام متوسط التكلفة الحالي
            var costAmount = line.BaseQuantity * line.Item.AverageCost;

            // تجميع بيانات القيد المحاسبي
            if (line.Item.StockGroup.InventoryAccountId == null)
                throw new BusinessException($"مجموعة الأصناف {line.Item.StockGroup.GroupNameAr} غير مربوطة بحساب مخزون.");

            if (line.Item.StockGroup.CostOfGoodsSoldAccountId == null)
                throw new BusinessException($"مجموعة الأصناف {line.Item.StockGroup.GroupNameAr} غير مربوطة بحساب تكلفة البضاعة المباعة.");

            // Debit COGS Account
            journalLines.Add(new JournalEntryLine(
                Guid.NewGuid(),
                Guid.Empty,
                line.Item.StockGroup.CostOfGoodsSoldAccountId.Value,
                costAmount,
                0,
                defaultCurrency.Id,
                1,
                null,
                $"صرف مخزون - {line.Item.ItemNameAr}"
            ));

            // Credit Inventory Account
            journalLines.Add(new JournalEntryLine(
                Guid.NewGuid(),
                Guid.Empty,
                line.Item.StockGroup.InventoryAccountId.Value,
                0,
                costAmount,
                defaultCurrency.Id,
                1,
                null,
                $"صرف مخزون - {line.Item.ItemNameAr}"
            ));
        }

        // تجميع الأسطر حسب الحساب
        var groupedLines = journalLines
            .GroupBy(l => l.AccountId)
            .Select(g => new JournalEntryLine(
                Guid.NewGuid(),
                Guid.Empty,
                g.Key,
                g.Sum(x => x.Debit),
                g.Sum(x => x.Credit),
                defaultCurrency.Id,
                1,
                null,
                $"إذن صرف رقم: {transaction.DocumentNumber}"
            ))
            .ToList();

        // إنشاء القيد المحاسبي
        await CreateAndPostJournalEntry(
            transaction,
            fiscalPeriodId,
            userId,
            groupedLines,
            cancellationToken);
    }

    private async Task ProcessTransfer(
        InventoryTransactionMaster transaction,
        Guid fiscalPeriodId,
        string userId,
        CancellationToken cancellationToken)
    {
        if (transaction.ToWarehouseId == null)
            throw new BusinessException("يجب تحديد المستودع الوجهة للتحويلات.");

        var journalLines = new List<JournalEntryLine>();

        // Get default currency
        var currencies = await _unitOfWork.Repository<Currency>().ListAllAsync();
        var defaultCurrency = currencies.FirstOrDefault();
        if (defaultCurrency == null)
            throw new BusinessException("لا توجد عملات في النظام.");

        foreach (var line in transaction.Lines)
        {
            if (line.Item == null)
                throw new BusinessException($"الصنف في السطر غير موجود.");

            if (line.Item.StockGroup == null)
                throw new BusinessException($"مجموعة الأصناف للصنف {line.Item.ItemNameAr} غير محددة.");

            // التحقق من الكمية المتوفرة في المستودع المصدر
            await ValidateStockAvailability(line.Item.Id, transaction.WarehouseId, line.BaseQuantity);

            // خصم من المستودع المصدر
            await UpdateBatchesForStockOut(line.ItemId, transaction.WarehouseId, line.BaseQuantity, line.BatchNumber);

            // إضافة للمستودع الوجهة
            await UpdateBatchesForStockIn(line.ItemId, transaction.ToWarehouseId.Value, line.BaseQuantity, line.Item.AverageCost, line.BatchNumber);

            // تجميع بيانات القيد المحاسبي
            if (line.Item.StockGroup.InventoryAccountId == null)
                throw new BusinessException($"مجموعة الأصناف {line.Item.StockGroup.GroupNameAr} غير مربوطة بحساب مخزون.");

            var costAmount = line.BaseQuantity * line.Item.AverageCost;

            // Debit Destination Warehouse Inventory
            journalLines.Add(new JournalEntryLine(
                Guid.NewGuid(),
                Guid.Empty,
                line.Item.StockGroup.InventoryAccountId.Value,
                costAmount,
                0,
                defaultCurrency.Id,
                1,
                null,
                $"تحويل مخزون للمستودع الوجهة - {line.Item.ItemNameAr}"
            ));

            // Credit Source Warehouse Inventory
            journalLines.Add(new JournalEntryLine(
                Guid.NewGuid(),
                Guid.Empty,
                line.Item.StockGroup.InventoryAccountId.Value,
                0,
                costAmount,
                defaultCurrency.Id,
                1,
                null,
                $"تحويل مخزون من المستودع المصدر - {line.Item.ItemNameAr}"
            ));
        }

        // تجميع الأسطر حسب الحساب
        var groupedLines = journalLines
            .GroupBy(l => l.AccountId)
            .Select(g => new JournalEntryLine(
                Guid.NewGuid(),
                Guid.Empty,
                g.Key,
                g.Sum(x => x.Debit),
                g.Sum(x => x.Credit),
                defaultCurrency.Id,
                1,
                null,
                $"إذن تحويل رقم: {transaction.DocumentNumber}"
            ))
            .ToList();

        // إنشاء القيد المحاسبي
        await CreateAndPostJournalEntry(
            transaction,
            fiscalPeriodId,
            userId,
            groupedLines,
            cancellationToken);
    }

    private async Task UpdateAverageCost(Item item, Guid? warehouseId, decimal newQuantity, decimal newPrice)
    {
        // حساب إجمالي الكمية والقيمة الحالية
        var currentStockQuantity = await GetCurrentStockQuantity(item.Id, warehouseId);
        var currentTotalValue = item.AverageCost * currentStockQuantity;
        var newTotalValue = newQuantity * newPrice;
        var totalQuantity = currentStockQuantity + newQuantity;

        if (totalQuantity > 0)
        {
            var newAverageCost = (currentTotalValue + newTotalValue) / totalQuantity;
            item.UpdateCosts(item.StandardCost, newAverageCost);
            _unitOfWork.Repository<Item>().Update(item);
        }
        else
        {
            item.UpdateCosts(newPrice, newPrice);
            _unitOfWork.Repository<Item>().Update(item);
        }
    }

    private async Task<decimal> GetCurrentStockQuantity(Guid itemId, Guid? warehouseId)
    {
        var spec = new ItemBatchByItemAndWarehouseSpecification(itemId, warehouseId);
        var batches = await _unitOfWork.Repository<ItemBatch>().ListAsync(spec);

        return batches.Sum(b => b.QuantityOnHand);
    }

    private async Task ValidateStockAvailability(Guid itemId, Guid? warehouseId, decimal requiredQuantity)
    {
        var availableQuantity = await GetAvailableStockQuantity(itemId, warehouseId);

        if (availableQuantity < requiredQuantity)
            throw new BusinessException($"عذراً، الكمية المطلوبة غير متوفرة في المستودع المحدد. المتوفر: {availableQuantity}، المطلوب: {requiredQuantity}");
    }

    private async Task<decimal> GetAvailableStockQuantity(Guid itemId, Guid? warehouseId)
    {
        var spec = new ItemBatchByItemAndWarehouseSpecification(itemId, warehouseId);
        var batches = await _unitOfWork.Repository<ItemBatch>().ListAsync(spec);

        return batches.Sum(b => b.QuantityOnHand);
    }

    private async Task UpdateBatchesForStockIn(Guid itemId, Guid? warehouseId, decimal quantity, decimal cost, string? batchNumber)
    {
        if (string.IsNullOrEmpty(batchNumber))
            throw new BusinessException("يجب تحديد رقم الدفعة للإضافة.");

        var batchSpec = new ItemBatchSpecification(itemId, warehouseId, batchNumber);
        var batch = await _unitOfWork.Repository<ItemBatch>().GetEntityWithSpec(batchSpec);

        if (batch != null)
        {
            // تحديث الدفعة الموجودة
            batch.UpdateQuantity(quantity);
            _unitOfWork.Repository<ItemBatch>().Update(batch);
        }
        else
        {
            // إنشاء دفعة جديدة
            var newBatch = new ItemBatch(
                Guid.NewGuid(),
                itemId,
                warehouseId,
                batchNumber,
                cost,
                quantity
            );
            _unitOfWork.Repository<ItemBatch>().Add(newBatch);
        }
    }

    private async Task UpdateBatchesForStockOut(Guid itemId, Guid? warehouseId, decimal quantity, string? batchNumber)
    {
        if (string.IsNullOrEmpty(batchNumber))
            throw new BusinessException("يجب تحديد رقم الدفعة للصرف.");

        var batchSpec = new ItemBatchSpecification(itemId, warehouseId, batchNumber);
        var batch = await _unitOfWork.Repository<ItemBatch>().GetEntityWithSpec(batchSpec);

        if (batch == null)
            throw new BusinessException($"الدفعة رقم ({batchNumber}) غير موجودة للصنف في المستودع المحدد.");

        if (batch.QuantityOnHand < quantity)
            throw new BusinessException($"الكمية غير كافية في الدفعة ({batchNumber}). المتوفر: {batch.QuantityOnHand}، المطلوب: {quantity}");

        // خصم الكمية من الدفعة
        batch.UpdateQuantity(-quantity);
        _unitOfWork.Repository<ItemBatch>().Update(batch);
    }

    private async Task CreateAndPostJournalEntry(
        InventoryTransactionMaster transaction,
        Guid fiscalPeriodId,
        string userId,
        List<JournalEntryLine> lines,
        CancellationToken cancellationToken)
    {
        var journalEntry = new JournalEntryMaster(
            Guid.NewGuid(),
            $"JV-INV-{transaction.DocumentNumber}",
            transaction.TransactionDate,
            $"قيد آلي ناتج عن حركة مخزنية رقم: {transaction.DocumentNumber} - نوع: {transaction.TransactionType}",
            fiscalPeriodId,
            userId
        );

        journalEntry.Post(userId);
        _unitOfWork.Repository<JournalEntryMaster>().Add(journalEntry);

        // Set the JournalEntryMasterId for each line and add to repository
        foreach (var line in lines)
        {
            var updatedLine = new JournalEntryLine(
                line.Id,
                journalEntry.Id,
                line.AccountId,
                line.Debit,
                line.Credit,
                line.CurrencyId,
                line.ExchangeRate,
                line.CostCenterId,
                line.Memo
            );
            _unitOfWork.Repository<JournalEntryLine>().Add(updatedLine);
        }

        // تحديث الأرصدة التراكمية باستخدام الخدمة المركزية
        // We need to pass the actual lines that were added to the repository
        var actualLines = lines.Select(l => new JournalEntryLine(
            l.Id,
            journalEntry.Id,
            l.AccountId,
            l.Debit,
            l.Credit,
            l.CurrencyId,
            l.ExchangeRate,
            l.CostCenterId,
            l.Memo
        )).ToList();

        await _accountBalanceService.UpdateBalancesForLinesAsync(
            actualLines,
            fiscalPeriodId,
            cancellationToken);
    }
}
