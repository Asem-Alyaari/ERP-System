using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.JournalEntries.Commands.Create;

public class CreateJournalEntryCommandHandler : IRequestHandler<CreateJournalEntryCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateJournalEntryCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateJournalEntryCommand request, CancellationToken cancellationToken)
    {
        // 1. التحقق من الفترة المالية
        var fiscalPeriod = await _unitOfWork.Repository<FiscalPeriod>().GetByIdAsync(request.FiscalPeriodId);
        if (fiscalPeriod == null)
            throw new BusinessException("الفترة المالية غير موجودة.");

        if (fiscalPeriod.IsClosed)
            throw new BusinessException("لا يمكن إضافة قيد في فترة مالية مغلقة.");

        // 2. التحقق من توازن القيد (Debit == Credit)
        var totalDebit = request.Lines.Sum(x => x.Debit);
        var totalCredit = request.Lines.Sum(x => x.Credit);

        if (totalDebit != totalCredit)
            throw new BusinessException($"القيد غير متوازن. إجمالي المدين: {totalDebit}، إجمالي الدائن: {totalCredit}");

        // 3. التحقق من أن الحسابات تحليلية (IsDetail == true)
        // 4. التحقق من قواعد مراكز التكلفة بناءً على CostCenterStatus
        foreach (var lineDto in request.Lines)
        {
            var account = await _unitOfWork.Repository<Account>().GetByIdAsync(lineDto.AccountId);
            if (account == null)
                throw new BusinessException($"الحساب ذو المعرف {lineDto.AccountId} غير موجود.");

            if (!account.IsDetail)
                throw new BusinessException($"الحساب ({account.AccountNameAr}) ليس حساباً تحليلياً. لا يمكن استخدامه في القيود.");

            // التحقق من قواعد مراكز التكلفة
            switch (account.CostCenterStatus)
            {
                case CostCenterStatus.Required:
                    // مركز التكلفة إلزامي - يجب أن يكون CostCenterId غير null وغير Guid.Empty
                    if (!lineDto.CostCenterId.HasValue || lineDto.CostCenterId.Value == Guid.Empty)
                        throw new BusinessException(
                            $"الحساب '{account.AccountNameAr}' ({account.AccountCode}) يتطلب مركز تكلفة إلزامياً. " +
                            $"يرجى تحديد مركز تكلفة لهذا السطر.");
                    break;

                case CostCenterStatus.Disabled:
                    // مركز التكلفة معطل - يجب أن يكون CostCenterId null
                    if (lineDto.CostCenterId.HasValue)
                        throw new BusinessException(
                            $"الحساب '{account.AccountNameAr}' ({account.AccountCode}) لديه مراكز التكلفة معطلة. " +
                            $"لا يمكن ربط مركز تكلفة بهذا السطر.");
                    break;

                case CostCenterStatus.Optional:
                    // مركز التكلفة اختياري - قبول المعاملة سواء كان CostCenterId موجوداً أو null
                    break;

                default:
                    throw new BusinessException($"حالة مركز التكلفة غير معروفة للحساب '{account.AccountNameAr}'.");
            }
        }

        // 5. إنشاء رأس القيد
        var entryMaster = new JournalEntryMaster(
            Guid.NewGuid(),
            request.VoucherNumber,
            request.TransactionDate,
            request.Description,
            request.FiscalPeriodId,
            request.CreatedBy
        );

        // البدء بالمعاملة (Transaction) لضمان حفظ الرأس والأسطر معاً
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            _unitOfWork.Repository<JournalEntryMaster>().Add(entryMaster);

            // 6. إضافة أسطر القيد
            foreach (var lineDto in request.Lines)
            {
                var line = new JournalEntryLine(
                    Guid.NewGuid(),
                    entryMaster.Id,
                    lineDto.AccountId,
                    lineDto.Debit,
                    lineDto.Credit,
                    lineDto.CurrencyId,
                    lineDto.ExchangeRate,
                    lineDto.CostCenterId,
                    lineDto.Memo,
                    lineDto.ForeignDebit,
                    lineDto.ForeignCredit
                );

                _unitOfWork.Repository<JournalEntryLine>().Add(line);
            }

            // إتمام الحفظ والـ Transaction
            await _unitOfWork.Complete();
            await _unitOfWork.CommitTransactionAsync();

            return entryMaster.Id;
        }
        catch (Exception)
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }
}
