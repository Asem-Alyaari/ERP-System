using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.Accounts.Commands.Create;

public record CreateAccountCommand : IRequest<Guid>
{
    public string AccountCode { get; init; } = string.Empty;
    public string AccountNameAr { get; init; } = string.Empty;
    public string AccountNameEn { get; init; } = string.Empty;
    public Guid? ParentAccountId { get; init; }
    public AccountType AccountType { get; init; }
    public bool IsDetail { get; init; }
    public Guid CurrencyId { get; init; }
}

public class CreateAccountCommandHandler : IRequestHandler<CreateAccountCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var account = new Account(
            Guid.NewGuid(),
            request.AccountCode,
            request.AccountNameAr,
            request.AccountNameEn,
            request.AccountType,
            request.IsDetail,
            request.CurrencyId,
            request.ParentAccountId
        );

        _unitOfWork.Repository<Account>().Add(account);
        await _unitOfWork.Complete();

        return account.Id;
    }
}
