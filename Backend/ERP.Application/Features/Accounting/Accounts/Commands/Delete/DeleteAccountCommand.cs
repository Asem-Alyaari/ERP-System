using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Accounting.Accounts.Commands.Delete;

public record DeleteAccountCommand(Guid Id) : IRequest<MediatR.Unit>;

public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAccountCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var account = await _unitOfWork.Repository<Account>().GetByIdAsync(request.Id);

        if (account == null)
        {
            throw new Exception($"Account with ID {request.Id} not found");
        }

        // Check if this account has any sub-accounts
        var allAccounts = await _unitOfWork.Repository<Account>().ListAllAsync();
        var hasSubAccounts = allAccounts.Any(a => a.ParentAccountId == account.Id);
        if (hasSubAccounts)
        {
            throw new Exception("لا يمكن حذف الحساب لأنه يحتوي على حسابات فرعية. يجب حذف الحسابات الفرعية أولاً.");
        }

        // The generic delete and complete will fail automatically if there are database constraints,
        // but we can proactively delete here.
        _unitOfWork.Repository<Account>().Delete(account);
        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
