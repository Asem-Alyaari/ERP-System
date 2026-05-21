using ERP.Application.Features.Accounting.AccountBalances.Specifications;
using ERP.Application.Features.Treasury.Vouchers.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Treasury.Vouchers.Commands.Post;

public record PostReceiptVoucherCommand(Guid VoucherId, string UserId) : IRequest<bool>;

public class PostReceiptVoucherCommandHandler : IRequestHandler<PostReceiptVoucherCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public PostReceiptVoucherCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(PostReceiptVoucherCommand request, CancellationToken cancellationToken)
    {
        var spec = new ReceiptVoucherWithDetailsSpecification(request.VoucherId);
        var voucher = await _unitOfWork.Repository<ReceiptVoucher>().GetEntityWithSpec(spec);

        if (voucher == null)
            throw new BusinessException("سند القبض غير موجود.");

        if (voucher.Status != VoucherStatus.Draft)
            throw new BusinessException($"لا يمكن ترحيل السند لأنه بحالة: {voucher.Status}");

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            var fiscalPeriod = (await _unitOfWork.Repository<FiscalPeriod>().ListAllAsync())
                .FirstOrDefault(p => !p.IsClosed);

            if (fiscalPeriod == null)
                throw new BusinessException("لا توجد فترة مالية مفتوحة لتوليد القيد المحاسبي.");

            // توليد القيد المحاسبي
            var journalEntry = new JournalEntryMaster(
                Guid.NewGuid(),
                $"JV-RV-{voucher.VoucherNumber}",
                voucher.VoucherDate,
                voucher.Notes ?? $"قيد ناتج عن سند قبض رقم: {voucher.VoucherNumber}",
                fiscalPeriod.Id,
                request.UserId
            );

            journalEntry.Post(request.UserId);
            _unitOfWork.Repository<JournalEntryMaster>().Add(journalEntry);

            // 1. الطرف المدين: الصندوق أو البنك (المستلم)
            var debitLine = new JournalEntryLine(
                Guid.NewGuid(), journalEntry.Id, voucher.DestinationAccountId,
                voucher.Amount, 0, Guid.Empty, 1, null, journalEntry.Description
            );
            _unitOfWork.Repository<JournalEntryLine>().Add(debitLine);
            await UpdateAccountBalance(debitLine, fiscalPeriod.Id);

            // 2. الطرف الدائن: المصدر (عميل أو حساب مباشر)
            Guid creditAccountId;
            if (voucher.SourceType == VoucherPartnerType.Customer)
            {
                if (voucher.Customer == null) throw new BusinessException("يجب تحديد العميل في حال كان نوع المصدر عميل.");
                creditAccountId = voucher.Customer.AccountId;
            }
            else
            {
                if (voucher.SourceAccountId == null) throw new BusinessException("يجب تحديد حساب المصدر.");
                creditAccountId = voucher.SourceAccountId.Value;
            }

            var creditLine = new JournalEntryLine(
                Guid.NewGuid(), journalEntry.Id, creditAccountId,
                0, voucher.Amount, Guid.Empty, 1, null, journalEntry.Description
            );
            _unitOfWork.Repository<JournalEntryLine>().Add(creditLine);
            await UpdateAccountBalance(creditLine, fiscalPeriod.Id);

            // تحديث حالة السند
            voucher.Post(request.UserId);
            _unitOfWork.Repository<ReceiptVoucher>().Update(voucher);

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
