using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Accounting.Accounts.Commands.Update;

public record UpdateAccountCommand : IRequest<MediatR.Unit>
{
    public Guid Id { get; init; }
    public string AccountNameAr { get; init; } = string.Empty;
    public string AccountNameEn { get; init; } = string.Empty;
    public bool IsDetail { get; init; }
    public Guid CurrencyId { get; init; }
}

public class UpdateAccountCommandHandler : IRequestHandler<UpdateAccountCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAccountCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(UpdateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _unitOfWork.Repository<Account>().GetByIdAsync(request.Id);

        if (account == null)
        {
            throw new Exception($"Account with ID {request.Id} not found");
        }

        // Business rule: If we are turning a parent account (IsDetail = false) into a detail account (IsDetail = true),
        // we must ensure it doesn't have any sub-accounts.
        if (request.IsDetail && !account.IsDetail)
        {
            // Check if there are any sub accounts
            var allAccounts = await _unitOfWork.Repository<Account>().ListAllAsync();
            var hasSubAccounts = allAccounts.Any(a => a.ParentAccountId == account.Id);
            if (hasSubAccounts)
            {
                throw new Exception("لا يمكن تحويل الحساب إلى حساب تحليلي لأنه يحتوي على حسابات فرعية.");
            }
        }

        account.UpdateDetails(
            request.AccountNameAr,
            request.AccountNameEn,
            request.IsDetail,
            request.CurrencyId
        );

        _unitOfWork.Repository<Account>().Update(account);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
