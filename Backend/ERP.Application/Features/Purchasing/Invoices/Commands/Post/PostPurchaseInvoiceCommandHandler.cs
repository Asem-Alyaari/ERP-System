using ERP.Application.Features.Accounting.AccountBalances.Specifications;
using ERP.Application.Features.Inventory.Transactions.Commands.Approve;
using ERP.Application.Features.Purchasing.Invoices.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Purchasing.Invoices.Commands.Post;

public record PostPurchaseInvoiceCommand(Guid InvoiceMasterId, string UserId) : IRequest<bool>;

public class PostPurchaseInvoiceCommandHandler : IRequestHandler<PostPurchaseInvoiceCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public PostPurchaseInvoiceCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<bool> Handle(PostPurchaseInvoiceCommand request, CancellationToken cancellationToken)
    {
        // 1. جلب الفاتورة مع التفاصيل
        var spec = new PurchaseInvoiceWithDetailsSpecification(request.InvoiceMasterId);
        var invoice = await _unitOfWork.Repository<PurchaseInvoiceMaster>().GetEntityWithSpec(spec);

        if (invoice == null)
            throw new BusinessException("فاتورة المشتريات غير موجودة.");

        if (invoice.Status != PurchaseInvoiceStatus.Draft)
            throw new BusinessException($"لا يمكن ترحيل الفاتورة لأنها بحالة: {invoice.Status}");

        // البدء بالمعاملة الشاملة
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // --- أولاً: الأثر المخزني (إنشاء واعتماد إذن إضافة) ---
            var inventoryMaster = new InventoryTransactionMaster(
                Guid.NewGuid(),
                $"SI-PI-{invoice.InvoiceNumber}",
                invoice.InvoiceDate,
                InventoryTransactionType.StockIn,
                request.UserId,
                $"إذن إضافة ناتج عن فاتورة مشتريات رقم: {invoice.InvoiceNumber}"
            );
            _unitOfWork.Repository<InventoryTransactionMaster>().Add(inventoryMaster);

            foreach (var invLine in invoice.Lines)
            {
                // جلب الصنف لضمان الحصول على معامل التحويل (في حال لم يكن مخزناً في سطر الفاتورة)
                // للتبسيط، سنعتمد على أن الوحدة المختارة في الفاتورة هي نفسها وحدة الصنف المستخدمة في المخزن
                // ولكن سنفترض معامل تحويل 1 إذا لم يتوفر نظام تحويل معقد هنا
                var inventoryLine = new InventoryTransactionLine(
                    Guid.NewGuid(),
                    inventoryMaster.Id,
                    invLine.ItemId,
                    invLine.UnitId,
                    invLine.Quantity,
                    1, // معامل التحويل الافتراضي
                    invLine.Price,
                    invLine.BatchNumber
                );
                _unitOfWork.Repository<InventoryTransactionLine>().Add(inventoryLine);
            }

            // اعتماد إذن الإضافة برمجياً (سيقوم بتحديث الدفعات وتوليد قيد المخزن)
            await _unitOfWork.Complete(); // حفظ المستند أولاً لكي يتمكن الـ Handler من جلبه
            await _mediator.Send(new ApproveStockInCommand(inventoryMaster.Id, request.UserId));

            // --- ثانياً: الأثر المالي (توليد القيد المالي المتمم) ---
            var journalEntry = new JournalEntryMaster(
                Guid.NewGuid(),
                $"JV-PI-{invoice.InvoiceNumber}",
                invoice.InvoiceDate,
                $"قيد استحقاق فاتورة مشتريات رقم: {invoice.InvoiceNumber} - مورد: {invoice.Vendor?.NameAr}",
                invoice.FiscalPeriodId,
                request.UserId
            );

            journalEntry.Post(request.UserId);
            _unitOfWork.Repository<JournalEntryMaster>().Add(journalEntry);

            // حسابات القيد:
            // 1. الطرف المدين: إغلاق حساب المشتريات الوسيط (الذي فتحه إذن الإضافة)
            // 2. الطرف المدين: حساب ضريبة القيمة المضافة
            // 3. الطرف الدائن: حساب المورد بالصافي

            // ملاحظة: حساب المشتريات الوسيط هو نفسه المستخدم في ApproveStockIn (CostOfGoodsSoldAccountId)
            // سنقوم بتجميع المبالغ حسب المجموعات المخزنية لإغلاق الحسابات الوسيطة
            var groupTotals = invoice.Lines
                .GroupBy(l => l.Item?.StockGroup)
                .Select(g => new { Group = g.Key, Total = g.Sum(x => x.Quantity * x.Price) });

            foreach (var gt in groupTotals)
            {
                if (gt.Group?.CostOfGoodsSoldAccountId == null)
                    throw new BusinessException($"مجموعة الأصناف {gt.Group?.GroupNameAr} غير مربوطة بحساب وسيط.");

                var debitOffsetLine = new JournalEntryLine(
                    Guid.NewGuid(), journalEntry.Id, gt.Group.CostOfGoodsSoldAccountId.Value,
                    gt.Total, 0, Guid.Empty, 1, null, "إغلاق حساب المشتريات الوسيط"
                );
                _unitOfWork.Repository<JournalEntryLine>().Add(debitOffsetLine);
                await UpdateAccountBalance(debitOffsetLine, invoice.FiscalPeriodId);
            }

            // الطرف المدين: ضريبة القيمة المضافة (استخدام حساب افتراضي للتأسيس)
            var vatAccountId = Guid.Parse("00000000-0000-0000-0000-000000000001"); // Placeholder لـ حساب الضريبة
            var vatLine = new JournalEntryLine(
                Guid.NewGuid(), journalEntry.Id, vatAccountId,
                invoice.TaxAmount, 0, Guid.Empty, 1, null, "ضريبة القيمة المضافة للمشتريات"
            );
            _unitOfWork.Repository<JournalEntryLine>().Add(vatLine);
            await UpdateAccountBalance(vatLine, invoice.FiscalPeriodId);

            // الطرف الدائن: المورد
            var vendorLine = new JournalEntryLine(
                Guid.NewGuid(), journalEntry.Id, invoice.Vendor!.AccountId,
                0, invoice.NetAmount, Guid.Empty, 1, null, $"استحقاق مورد: {invoice.Vendor.NameAr}"
            );
            _unitOfWork.Repository<JournalEntryLine>().Add(vendorLine);
            await UpdateAccountBalance(vendorLine, invoice.FiscalPeriodId);

            // --- ثالثاً: تحديث حالة الفاتورة ---
            invoice.Post(request.UserId);
            _unitOfWork.Repository<PurchaseInvoiceMaster>().Update(invoice);

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
