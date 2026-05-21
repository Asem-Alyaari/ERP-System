using ERP.Application.Features.Accounting.Accounts.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Accounting.Accounts.Queries.GetAccountsTree;

public record GetAccountsTreeQuery : IRequest<List<AccountDto>>;

public class AccountDto
{
    public Guid Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountNameAr { get; set; } = string.Empty;
    public string AccountNameEn { get; set; } = string.Empty;
    public Guid? ParentAccountId { get; set; }
    public AccountType AccountType { get; set; }
    public string AccountTypeName => AccountType.ToString();
    public bool IsDetail { get; set; }
    public Guid CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencySymbol { get; set; } = string.Empty;
}

public class GetAccountsTreeQueryHandler : IRequestHandler<GetAccountsTreeQuery, List<AccountDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAccountsTreeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AccountDto>> Handle(GetAccountsTreeQuery request, CancellationToken cancellationToken)
    {
        var spec = new AccountsWithCurrencySpecification();
        var accounts = await _unitOfWork.Repository<Account>().ListAsync(spec);

        return accounts.Select(a => new AccountDto
        {
            Id = a.Id,
            AccountCode = a.AccountCode,
            AccountNameAr = a.AccountNameAr,
            AccountNameEn = a.AccountNameEn,
            ParentAccountId = a.ParentAccountId,
            AccountType = a.AccountType,
            IsDetail = a.IsDetail,
            CurrencyId = a.CurrencyId,
            CurrencyCode = a.Currency?.Code ?? string.Empty,
            CurrencySymbol = a.Currency?.Symbol ?? string.Empty
        }).ToList();
    }
}
