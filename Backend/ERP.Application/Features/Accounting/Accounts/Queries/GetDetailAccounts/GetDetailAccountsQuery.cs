using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ERP.Application.Features.Accounting.Accounts.Queries.GetDetailAccounts;

public record GetDetailAccountsQuery : IRequest<List<AccountLookupDto>>;

public class AccountLookupDto
{
    public Guid Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountNameAr { get; set; } = string.Empty;
    public string AccountNameEn { get; set; } = string.Empty;
    public string CostCenterStatus { get; set; } = "Optional";
}

public class GetDetailAccountsQueryHandler : IRequestHandler<GetDetailAccountsQuery, List<AccountLookupDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetDetailAccountsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AccountLookupDto>> Handle(GetDetailAccountsQuery request, CancellationToken cancellationToken)
    {
        var accounts = await _unitOfWork.Repository<Account>().ListAllAsync();

        // Filter detail accounts only
        return accounts
            .Where(a => a.IsDetail)
            .Select(a => new AccountLookupDto
            {
                Id = a.Id,
                AccountCode = a.AccountCode,
                AccountNameAr = a.AccountNameAr,
                AccountNameEn = a.AccountNameEn,
                CostCenterStatus = a.CostCenterStatus.ToString()
            })
            .OrderBy(a => a.AccountCode)
            .ToList();
    }
}
