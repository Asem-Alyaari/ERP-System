using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.Reports.Queries.GetAccountStatement;

public record GetAccountStatementQuery(Guid AccountId, DateTime? FromDate, DateTime? ToDate) : IRequest<AccountStatementDto>;

public class GetAccountStatementQueryHandler : IRequestHandler<GetAccountStatementQuery, AccountStatementDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetAccountStatementQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountStatementDto> Handle(GetAccountStatementQuery request, CancellationToken cancellationToken)
    {
        var result = new AccountStatementDto();

        // 1. حساب الرصيد الافتتاحي (قبل التاريخ المحدد)
        if (request.FromDate.HasValue)
        {
            var openingSpec = new AccountOpeningBalanceSpecification(request.AccountId, request.FromDate.Value);
            var openingLines = await _unitOfWork.Repository<JournalEntryLine>().ListAsync(openingSpec);
            result.OpeningBalance = openingLines.Sum(x => x.Debit - x.Credit);
        }
        else
        {
            result.OpeningBalance = 0;
        }

        // 2. جلب الحركات التفصيلية للفترة
        var statementSpec = new AccountStatementSpecification(request.AccountId, request.FromDate, request.ToDate);
        var lines = await _unitOfWork.Repository<JournalEntryLine>().ListAsync(statementSpec);

        decimal runningBalance = result.OpeningBalance;

        foreach (var line in lines)
        {
            runningBalance += (line.Debit - line.Credit);

            result.StatementLines.Add(new AccountStatementLineDto
            {
                TransactionDate = line.JournalEntryMaster!.TransactionDate,
                DocumentNumber = line.JournalEntryMaster.VoucherNumber,
                Description = line.Memo ?? line.JournalEntryMaster.Description,
                Debit = line.Debit,
                Credit = line.Credit,
                RunningBalance = runningBalance
            });
        }

        // 3. الرصيد الختامي
        result.ClosingBalance = runningBalance;

        return result;
    }
}
