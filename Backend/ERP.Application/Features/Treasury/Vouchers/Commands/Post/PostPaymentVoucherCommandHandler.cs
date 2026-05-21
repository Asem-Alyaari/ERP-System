using ERP.Application.Features.Accounting.AccountBalances.Specifications;
using ERP.Application.Features.Treasury.Vouchers.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Exceptions;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Treasury.Vouchers.Commands.Post;

public record PostPaymentVoucherCommand(Guid VoucherId, string UserId) : IRequest<bool>;

public class PostPaymentVoucherCommandHandler : IRequestHandler<PostPaymentVoucherCommand, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public PostPaymentVoucherCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(PostPaymentVoucherCommand request, CancellationToken cancellationToken)
    {
        var spec = new PaymentVoucherWithDetailsSpecification(request.VoucherId);
        var voucher = await _unitOfWork.Repository<PaymentVoucher>().GetEntityWithSpec(spec);

        if (voucher == null)
            throw new BusinessException("سند الصرف غير موجود.");

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
                $"JV-PV-{voucher.VoucherNumber}",
                voucher.VoucherDate,
                voucher.Notes ?? $"قيد ناتج عن سند صرف رقم: {voucher.VoucherNumber}",
                fiscalPeriod.Id,
                request.UserId
            );

            journalEntry.Post(request.UserId);
            _unitOfWork.Repository<JournalEntryMaster>().Add(journalEntry);

            // 1. الطرف الدائن: الصندوق أو البنك (المصدر)
            var creditLine = new JournalEntryLine(
                Guid.NewGuid(), journalEntry.Id, voucher.SourceAccountId,
                0, voucher.Amount, Guid.Empty, 1, null, journalEntry.Description
            );
            _unitOfWork.Repository<JournalEntryLine>().Add(creditLine);
            await UpdateAccountBalance(creditLine, fiscalPeriod.Id);

            // 2. الطرف المدين: الوجهة (مورد أو حساب مباشر)
            Guid debitAccountId;
            if (voucher.DestinationType == VoucherPartnerType.Vendor)
            {
                if (voucher.Vendor == null) throw new BusinessException("يجب تحديد المورد في حال كان نوع الوجهة مورد.");
                debitAccountId = voucher.Vendor.AccountId;
            }
            else
            {
                if (voucher.DestinationAccountId == null) throw new BusinessException("يجب تحديد حساب الوجهة.");
                debitAccountId = voucher.DestinationAccountId.Value;
            }

            var debitLine = new JournalEntryLine(
                Guid.NewGuid(), journalEntry.Id, debitAccountId,
                voucher.Amount, 0, Guid.Empty, 1, null, journalEntry.Description
            );
            _unitOfWork.Repository<JournalEntryLine>().Add(debitLine);
            await UpdateAccountBalance(debitLine, fiscalPeriod.Id);

            // تحديث حالة السند
            voucher.Post(request.UserId);
            _unitOfWork.Repository<PaymentVoucher>().Update(voucher);

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
