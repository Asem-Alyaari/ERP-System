using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Expenses.Bills.Commands.Create;

public class CreateExpenseBillCommandHandler : IRequestHandler<CreateExpenseBillCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateExpenseBillCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateExpenseBillCommand request, CancellationToken cancellationToken)
    {
        // 1. Parse TransactionDate
        if (!DateTime.TryParse(request.TransactionDate, out var transactionDate))
            throw new BusinessException("صيغة التاريخ غير صالحة.");

        // 2. التحقق من المبالغ
        if (request.TotalAmount <= 0)
            throw new BusinessException("المبلغ الإجمالي يجب أن يكون أكبر من صفر.");

        if (request.TaxAmount < 0)
            throw new BusinessException("مبلغ الضريبة لا يمكن أن يكون سالباً.");

        if (request.NetAmount <= 0)
            throw new BusinessException("المبلغ الصافي يجب أن يكون أكبر من صفر.");

        // Parse GUIDs
        Guid? vendorId = null;
        if (!string.IsNullOrEmpty(request.VendorId))
        {
            if (!Guid.TryParse(request.VendorId, out var parsedVendorId))
                throw new BusinessException("معرف المورد غير صالح.");
            vendorId = parsedVendorId;
        }

        Guid? paymentAccountId = null;
        if (!string.IsNullOrEmpty(request.PaymentAccountId))
        {
            if (!Guid.TryParse(request.PaymentAccountId, out var parsedPaymentAccountId))
                throw new BusinessException("معرف حساب الدفع غير صالح.");
            paymentAccountId = parsedPaymentAccountId;
        }

        // 3. التحقق من طريقة الدفع والبيانات المطلوبة
        if (request.PaymentMethod == ExpenseBillPaymentMethod.Credit)
        {
            // للمصروفات الآجلة، يجب تحديد المورد
            if (!vendorId.HasValue || vendorId.Value == Guid.Empty)
                throw new BusinessException("يجب تحديد المورد للمصروفات الآجلة.");

            var vendor = await _unitOfWork.Repository<Vendor>().GetByIdAsync(vendorId.Value);
            if (vendor == null)
                throw new BusinessException("المورد غير موجود.");
        }
        else if (request.PaymentMethod == ExpenseBillPaymentMethod.Cash || request.PaymentMethod == ExpenseBillPaymentMethod.Bank)
        {
            // للمصروفات النقدية أو البنكية، يجب تحديد حساب الدفع
            if (!paymentAccountId.HasValue || paymentAccountId.Value == Guid.Empty)
                throw new BusinessException("يجب تحديد حساب الدفع للمصروفات النقدية أو البنكية.");

            var paymentAccount = await _unitOfWork.Repository<Account>().GetByIdAsync(paymentAccountId.Value);
            if (paymentAccount == null)
                throw new BusinessException("حساب الدفع غير موجود.");

            // التحقق من أن الحساب هو أصل (خزينة أو بنك)
            if (paymentAccount.AccountType != AccountType.Asset)
                throw new BusinessException("حساب الدفع يجب أن يكون أصل (خزينة أو بنك).");
        }

        // 4. التحقق من وجود سطور
        if (request.Lines == null || !request.Lines.Any())
            throw new BusinessException("يجب إضافة سطر واحد على الأقل لفاتورة المصروفات.");

        // 5. التحقق من السطور
        foreach (var line in request.Lines)
        {
            if (line.Amount <= 0)
                throw new BusinessException("مبلغ السطر يجب أن يكون أكبر من صفر.");

            // Parse line GUIDs
            if (!Guid.TryParse(line.AccountId, out var accountId))
                throw new BusinessException("معرف الحساب غير صالح.");

            if (!Guid.TryParse(line.CostCenterId, out var costCenterId))
                throw new BusinessException("معرف مركز التكلفة غير صالح.");

            // التحقق من الحساب
            var account = await _unitOfWork.Repository<Account>().GetByIdAsync(accountId);
            if (account == null)
                throw new BusinessException($"الحساب برقم {line.AccountId} غير موجود.");

            // التحقق من أن الحساب هو حساب مصروفات (يبدأ بـ 5)
            if (!account.AccountCode.StartsWith("5"))
                throw new BusinessException($"الحساب '{account.AccountNameAr}' ({account.AccountCode}) ليس حساب مصروفات. يجب أن يبدأ برمز 5.");

            // التحقق من مركز التكلفة (إلزامي للمصروفات)
            if (costCenterId == Guid.Empty)
                throw new BusinessException($"الحساب '{account.AccountNameAr}' ({account.AccountCode}) يتطلب مركز تكلفة إلزامياً.");

            var costCenter = await _unitOfWork.Repository<CostCenter>().GetByIdAsync(costCenterId);
            if (costCenter == null)
                throw new BusinessException("مركز التكلفة غير موجود.");
        }

        // 6. التحقق من تطابق المبالغ
        var linesTotal = request.Lines.Sum(l => l.Amount);
        if (Math.Abs(linesTotal - request.NetAmount) > 0.01m)
            throw new BusinessException($"مجموع السطور ({linesTotal}) لا يساوي المبلغ الصافي ({request.NetAmount}).");

        // البدء بالمعاملة
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 7. إنشاء فاتورة المصروفات
            var expenseBill = new ExpenseBillMaster(
                Guid.NewGuid(),
                request.BillNumber,
                transactionDate,
                request.PaymentMethod,
                request.TotalAmount,
                request.TaxAmount,
                request.NetAmount,
                request.CreatedBy,
                request.Notes,
                vendorId,
                request.SupplierName,
                paymentAccountId
            );

            // 8. إضافة السطور
            foreach (var line in request.Lines)
            {
                Guid.TryParse(line.AccountId, out var accountId);
                Guid.TryParse(line.CostCenterId, out var costCenterId);

                var expenseBillLine = new ExpenseBillLine(
                    Guid.NewGuid(),
                    expenseBill.Id,
                    accountId,
                    line.Amount,
                    costCenterId,
                    line.Notes
                );
                expenseBill.AddLine(expenseBillLine);
            }

            _unitOfWork.Repository<ExpenseBillMaster>().Add(expenseBill);
            await _unitOfWork.Complete();
            await _unitOfWork.CommitTransactionAsync();

            return expenseBill.Id;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
