using ERP.Application.Features.Inventory.Transactions.Commands.Approve;
using ERP.Application.Features.Sales.Invoices.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using ERP.Domain.Services;
using MediatR;

namespace ERP.Application.Features.Sales.Invoices.Commands.Post;

public record PostSalesInvoiceCommand(Guid InvoiceMasterId, string UserId) : IRequest<bool>;

public class PostSalesInvoiceCommandHandler : IRequestHandler<PostSalesInvoiceCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;
    private readonly IAccountBalanceService _accountBalanceService;

    public PostSalesInvoiceCommandHandler(
        IUnitOfWork unitOfWork,
        IMediator mediator,
        IAccountBalanceService accountBalanceService)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
        _accountBalanceService = accountBalanceService;
    }

    public async Task<bool> Handle(PostSalesInvoiceCommand request, CancellationToken cancellationToken)
    {
        // 1. جلب الفاتورة مع التفاصيل (الأصناف والمجموعات المخزنية والعميل)
        var spec = new SalesInvoiceWithDetailsSpecification(request.InvoiceMasterId);
        var invoice = await _unitOfWork.Repository<SalesInvoiceMaster>().GetEntityWithSpec(spec);

        if (invoice == null)
            throw new BusinessException("فاتورة المبيعات غير موجودة.");

        if (invoice.Status != SalesInvoiceStatus.Draft)
            throw new BusinessException($"لا يمكن ترحيل الفاتورة لأنها بحالة: {invoice.Status}");

        // التحقق من أن تاريخ الفاتورة لا ينتمي إلى فترة مالية مغلقة
        var allPeriods = await _unitOfWork.Repository<FiscalPeriod>().ListAllAsync();
        var closedPeriod = allPeriods
            .FirstOrDefault(p => p.IsClosed && 
                               invoice.InvoiceDate >= p.StartDate && 
                               invoice.InvoiceDate <= p.EndDate);

        if (closedPeriod != null)
            throw new BusinessException("لا يمكن إجراء عمليات على فترة مالية مغلقة.");

        // --- أولاً: الأثر المخزني وتكلفة البضاعة (إنشاء واعتماد إذن صرف) ---
        var inventoryMaster = new InventoryTransactionMaster(
            Guid.NewGuid(),
            $"SO-SI-{invoice.InvoiceNumber}",
            invoice.InvoiceDate,
            InventoryTransactionType.StockOut,
            request.UserId,
            $"إذن صرف ناتج عن فاتورة مبيعات رقم: {invoice.InvoiceNumber}"
        );
        _unitOfWork.Repository<InventoryTransactionMaster>().Add(inventoryMaster);

        foreach (var invLine in invoice.Lines)
        {
            var inventoryLine = new InventoryTransactionLine(
                Guid.NewGuid(),
                inventoryMaster.Id,
                invLine.ItemId,
                invLine.UnitId,
                invLine.Quantity,
                1, // معامل التحويل الافتراضي (يمكن تطويره لجلب المعامل الفعلي من وحدات الصنف)
                invLine.Price, // سعر البيع (للتوثيق في إذن الصرف، لكن التكلفة ستحسب من الدفعة)
                invLine.BatchNumber
            );
            _unitOfWork.Repository<InventoryTransactionLine>().Add(inventoryLine);
        }

        // حفظ المستند المخزني أولاً لكي يتمكن الـ Handler من جلبه
        await _unitOfWork.Complete();

        // اعتماد إذن الصرف (سيقوم بفحص الكميات، خصم الدفعات، وتوليد قيد التكلفة COGS آلياً)
        await _mediator.Send(new ApproveStockOutCommand(inventoryMaster.Id, request.UserId), cancellationToken);

        // --- ثانياً: الأثر المالي للمبيعات (توليد قيد الإيرادات والمديونية) ---
        var fiscalPeriod = (await _unitOfWork.Repository<FiscalPeriod>().ListAllAsync())
            .FirstOrDefault(p => !p.IsClosed);

        if (fiscalPeriod == null)
            throw new BusinessException("لا توجد فترة مالية مفتوحة لتوليد القيد المحاسبي.");

        var journalEntry = new JournalEntryMaster(
            Guid.NewGuid(),
            $"JV-SI-{invoice.InvoiceNumber}",
            invoice.InvoiceDate,
            $"قيد إيرادات فاتورة مبيعات رقم: {invoice.InvoiceNumber} - عميل: {invoice.Customer?.NameAr}",
            fiscalPeriod.Id,
            request.UserId
        );

        journalEntry.Post(request.UserId);
        _unitOfWork.Repository<JournalEntryMaster>().Add(journalEntry);

        var journalLines = new List<JournalEntryLine>();

        // أطراف القيد:
        // 1. الجانب المدين: العميل بالصافي
        var customerLine = new JournalEntryLine(
            Guid.NewGuid(), journalEntry.Id, invoice.Customer!.AccountId,
            invoice.NetAmount, 0, Guid.Empty, 1, null, $"استحقاق على العميل: {invoice.Customer.NameAr}"
        );
        journalLines.Add(customerLine);

        // 2. الجانب الدائن: إيرادات المبيعات (تجميع حسب مجموعات الأصناف)
        var revenueGroups = invoice.Lines
            .GroupBy(l => l.Item?.StockGroup)
            .Select(g => new { Group = g.Key, Total = g.Sum(x => x.Quantity * x.Price) });

        foreach (var rg in revenueGroups)
        {
            if (rg.Group?.SalesAccountId == null)
                throw new BusinessException($"مجموعة الأصناف {rg.Group?.GroupNameAr} غير مربوطة بحساب إيرادات.");

            var revenueLine = new JournalEntryLine(
                Guid.NewGuid(), journalEntry.Id, rg.Group.SalesAccountId.Value,
                0, rg.Total, Guid.Empty, 1, null, $"إيرادات مبيعات - {rg.Group.GroupNameAr}"
            );
            journalLines.Add(revenueLine);
        }

        // 3. الجانب الدائن: ضريبة القيمة المضافة مخرجات
        var outputVatAccountId = Guid.Parse("00000000-0000-0000-0000-000000000002"); // Placeholder لـ حساب ضريبة المبيعات
        var vatLine = new JournalEntryLine(
            Guid.NewGuid(), journalEntry.Id, outputVatAccountId,
            0, invoice.TaxAmount, Guid.Empty, 1, null, "ضريبة القيمة المضافة مخرجات"
        );
        journalLines.Add(vatLine);

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

        // --- ثالثاً: تحديث حالة الفاتورة ---
        invoice.Post(request.UserId);
        _unitOfWork.Repository<SalesInvoiceMaster>().Update(invoice);

        // حفظ التغييرات (TransactionBehavior سيتولى إدارة Transaction)
        await _unitOfWork.Complete();

        return true;
    }
}
