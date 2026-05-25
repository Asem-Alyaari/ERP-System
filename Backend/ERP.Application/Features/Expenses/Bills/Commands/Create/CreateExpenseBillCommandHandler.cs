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
        // 1. التحقق من المبالغ
        if (request.TotalAmount <= 0)
            throw new BusinessException("المبلغ الإجمالي يجب أن يكون أكبر من صفر.");

        if (request.TaxAmount < 0)
            throw new BusinessException("مبلغ الضريبة لا يمكن أن يكون سالباً.");

        if (request.NetAmount <= 0)
            throw new BusinessException("المبلغ الصافي يجب أن يكون أكبر من صفر.");

        // 2. التحقق من طريقة الدفع والبيانات المطلوبة
        if (request.PaymentMethod == ExpenseBillPaymentMethod.Credit)
        {
            // للمصروفات الآجلة، يجب تحديد المورد
            if (!request.VendorId.HasValue || request.VendorId.Value == Guid.Empty)
                throw new BusinessException("يجب تحديد المورد للمصروفات الآجلة.");

            var vendor = await _unitOfWork.Repository<Vendor>().GetByIdAsync(request.VendorId.Value);
            if (vendor == null)
                throw new BusinessException("المورد غير موجود.");
        }
        else if (request.PaymentMethod == ExpenseBillPaymentMethod.Cash || request.PaymentMethod == ExpenseBillPaymentMethod.Bank)
        {
            // للمصروفات النقدية أو البنكية، يجب تحديد حساب الدفع
            if (!request.PaymentAccountId.HasValue || request.PaymentAccountId.Value == Guid.Empty)
                throw new BusinessException("يجب تحديد حساب الدفع للمصروفات النقدية أو البنكية.");

            var paymentAccount = await _unitOfWork.Repository<Account>().GetByIdAsync(request.PaymentAccountId.Value);
            if (paymentAccount == null)
                throw new BusinessException("حساب الدفع غير موجود.");

            // التحقق من أن الحساب هو أصل (خزينة أو بنك)
            if (paymentAccount.AccountType != AccountType.Asset)
                throw new BusinessException("حساب الدفع يجب أن يكون أصل (خزينة أو بنك).");
        }

        // 3. التحقق من وجود سطور
        if (request.Lines == null || !request.Lines.Any())
            throw new BusinessException("يجب إضافة سطر واحد على الأقل لفاتورة المصروفات.");

        // 4. التحقق من السطور
        foreach (var line in request.Lines)
        {
            if (line.Amount <= 0)
                throw new BusinessException("مبلغ السطر يجب أن يكون أكبر من صفر.");

            // التحقق من الحساب
            var account = await _unitOfWork.Repository<Account>().GetByIdAsync(line.AccountId);
            if (account == null)
                throw new BusinessException($"الحساب برقم {line.AccountId} غير موجود.");

            // التحقق من أن الحساب هو حساب مصروفات (يبدأ بـ 5)
            if (!account.AccountCode.StartsWith("5"))
                throw new BusinessException($"الحساب '{account.AccountNameAr}' ({account.AccountCode}) ليس حساب مصروفات. يجب أن يبدأ برمز 5.");

            // التحقق من مركز التكلفة (إلزامي للمصروفات)
            if (line.CostCenterId == Guid.Empty)
                throw new BusinessException($"الحساب '{account.AccountNameAr}' ({account.AccountCode}) يتطلب مركز تكلفة إلزامياً.");

            var costCenter = await _unitOfWork.Repository<CostCenter>().GetByIdAsync(line.CostCenterId);
            if (costCenter == null)
                throw new BusinessException("مركز التكلفة غير موجود.");
        }

        // 5. التحقق من تطابق المبالغ
        var linesTotal = request.Lines.Sum(l => l.Amount);
        if (Math.Abs(linesTotal - request.NetAmount) > 0.01m)
            throw new BusinessException($"مجموع السطور ({linesTotal}) لا يساوي المبلغ الصافي ({request.NetAmount}).");

        // البدء بالمعاملة
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // 6. إنشاء فاتورة المصروفات
            var expenseBill = new ExpenseBillMaster(
                Guid.NewGuid(),
                request.BillNumber,
                request.TransactionDate,
                request.PaymentMethod,
                request.TotalAmount,
                request.TaxAmount,
                request.NetAmount,
                request.CreatedBy,
                request.Notes,
                request.VendorId,
                request.SupplierName,
                request.PaymentAccountId
            );

            // 7. إضافة السطور
            foreach (var line in request.Lines)
            {
                var expenseBillLine = new ExpenseBillLine(
                    Guid.NewGuid(),
                    expenseBill.Id,
                    line.AccountId,
                    line.Amount,
                    line.CostCenterId,
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
