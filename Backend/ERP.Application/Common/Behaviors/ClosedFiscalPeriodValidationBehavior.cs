using ERP.Domain.Entities;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ERP.Application.Common.Behaviors;

/// <summary>
/// سلوك MediatR للتحقق من أن العمليات المالية لا تتم على فترات مالية مغلقة
/// يمنع أي عملية (إنشاء، تعديل، ترحيل) على فترة مالية مغلقة
/// </summary>
public class ClosedFiscalPeriodValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ClosedFiscalPeriodValidationBehavior<TRequest, TResponse>> _logger;

    public ClosedFiscalPeriodValidationBehavior(
        IUnitOfWork unitOfWork,
        ILogger<ClosedFiscalPeriodValidationBehavior<TRequest, TResponse>> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // تحديد ما إذا كان الطلب يتطلب التحقق من الفترة المالية
        var requiresValidation = RequiresFiscalPeriodValidation(typeof(TRequest));

        if (!requiresValidation)
        {
            return await next();
        }

        // استخراج معرف الفترة المالية من الطلب
        var fiscalPeriodId = ExtractFiscalPeriodId(request);

        if (fiscalPeriodId != null)
        {
            // التحقق من الفترة المالية باستخدام المعرف
            await ValidateFiscalPeriod(fiscalPeriodId.Value, typeof(TRequest).Name);
        }
        else
        {
            // إذا لم يتمكن من استخراج معرف الفترة، حاول استخراج التاريخ
            var transactionDate = ExtractTransactionDate(request);
            if (transactionDate != null)
            {
                // التحقق من الفترة المالية باستخدام التاريخ
                await ValidateFiscalPeriodByDate(transactionDate.Value, typeof(TRequest).Name);
            }
        }

        return await next();
    }

    /// <summary>
    /// التحقق من أن الفترة المالية ليست مغلقة
    /// </summary>
    private async Task ValidateFiscalPeriod(Guid fiscalPeriodId, string requestName)
    {
        var fiscalPeriod = await _unitOfWork.Repository<FiscalPeriod>()
            .GetByIdAsync(fiscalPeriodId);

        if (fiscalPeriod == null)
            throw new BusinessException("الفترة المالية غير موجودة.");

        if (fiscalPeriod.IsClosed)
        {
            _logger.LogWarning(
                "محاولة إجراء عملية على فترة مالية مغلقة: {FiscalPeriodId} - {RequestType}",
                fiscalPeriodId,
                requestName);

            throw new BusinessException("لا يمكن إجراء عمليات على فترة مالية مغلقة.");
        }
    }

    /// <summary>
    /// التحقق من أن التاريخ لا ينتمي إلى فترة مالية مغلقة
    /// </summary>
    private async Task ValidateFiscalPeriodByDate(DateTime transactionDate, string requestName)
    {
        var allPeriods = await _unitOfWork.Repository<FiscalPeriod>()
            .ListAllAsync();

        var closedPeriod = allPeriods
            .FirstOrDefault(p => p.IsClosed && 
                               transactionDate >= p.StartDate && 
                               transactionDate <= p.EndDate);

        if (closedPeriod != null)
        {
            _logger.LogWarning(
                "محاولة إجراء عملية بتاريخ ينتمي إلى فترة مالية مغلقة: {TransactionDate} - {FiscalPeriodId} - {RequestType}",
                transactionDate,
                closedPeriod.Id,
                requestName);

            throw new BusinessException("لا يمكن إجراء عمليات على فترة مالية مغلقة.");
        }
    }

    /// <summary>
    /// تحديد ما إذا كان الطلب يتطلب التحقق من الفترة المالية
    /// </summary>
    private bool RequiresFiscalPeriodValidation(Type requestType)
    {
        // القيود التي تتطلب التحقق من الفترة المالية
        var financialCommands = new[]
        {
            "PostJournalEntryCommand",
            "UnpostJournalEntryCommand",
            "CreateJournalEntryCommand",
            "PostSalesInvoiceCommand",
            "PostPurchaseInvoiceCommand",
            "PostPaymentVoucherCommand",
            "PostReceiptVoucherCommand",
            "PostExpenseBillCommand",
            "SetOpeningBalancesCommand",
            "PostInventoryTransactionCommand",
            "CreateInventoryTransactionCommand"
        };

        var requestName = requestType.Name;
        return financialCommands.Any(cmd => requestName.Contains(cmd));
    }

    /// <summary>
    /// استخراج معرف الفترة المالية من الطلب
    /// </summary>
    private Guid? ExtractFiscalPeriodId(TRequest request)
    {
        var requestType = request.GetType();
        
        // البحث عن خاصية FiscalPeriodId
        var fiscalPeriodIdProperty = requestType.GetProperty("FiscalPeriodId");
        if (fiscalPeriodIdProperty != null)
        {
            var value = fiscalPeriodIdProperty.GetValue(request);
            if (value is Guid guidValue && guidValue != Guid.Empty)
            {
                return guidValue;
            }
        }

        // للقيود، قد نحتاج للبحث عن JournalEntryId ثم جلب الفترة من القيد
        // لكن هذا يتطلب استعلام إضافي، لذا سنترك التحقق في الـ Handler نفسه
        // للقيود التي تحتاج FiscalPeriodId من القيد
        return null;
    }

    /// <summary>
    /// استخراج تاريخ المعاملة من الطلب
    /// </summary>
    private DateTime? ExtractTransactionDate(TRequest request)
    {
        var requestType = request.GetType();
        
        // البحث عن خصائص التاريخ الشائعة
        var dateProperties = new[] { "TransactionDate", "InvoiceDate", "VoucherDate", "Date" };
        
        foreach (var propName in dateProperties)
        {
            var prop = requestType.GetProperty(propName);
            if (prop != null)
            {
                var value = prop.GetValue(request);
                if (value is DateTime dateValue)
                {
                    return dateValue;
                }
            }
        }

        return null;
    }
}
