using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.Reports.Queries.GetTrialBalance;

public class GetTrialBalanceQueryHandler : IRequestHandler<GetTrialBalanceQuery, TrialBalanceResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTrialBalanceQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TrialBalanceResponse> Handle(GetTrialBalanceQuery request, CancellationToken cancellationToken)
    {
        // استخدام الـ Specification لجلب الأرصدة التراكمية مع بيانات الحسابات
        var spec = new TrialBalanceSpecification(request.FiscalPeriodId, null, null);
        var balances = await _unitOfWork.Repository<AccountBalance>().ListAsync(spec);

        var response = new TrialBalanceResponse();

        foreach (var balance in balances)
        {
            var line = new TrialBalanceDto
            {
                AccountId = balance.AccountId,
                AccountCode = balance.Account?.AccountCode ?? string.Empty,
                AccountNameAr = balance.Account?.AccountNameAr ?? string.Empty,
                AccountNameEn = balance.Account?.AccountNameEn ?? string.Empty,
                TotalDebit = balance.TotalDebit,
                TotalCredit = balance.TotalCredit
            };

            // احتساب الرصيد الصافي (مدين أو دائن)
            if (line.TotalDebit >= line.TotalCredit)
            {
                line.DebitBalance = line.TotalDebit - line.TotalCredit;
                line.CreditBalance = 0;
            }
            else
            {
                line.CreditBalance = line.TotalCredit - line.TotalDebit;
                line.DebitBalance = 0;
            }

            response.TrialBalanceLines.Add(line);
        }

        // ترتيب الحسابات حسب الكود
        response.TrialBalanceLines = response.TrialBalanceLines.OrderBy(x => x.AccountCode).ToList();

        // احتساب الإجماليات الكبرى للميزان
        response.SumTotalDebit = response.TrialBalanceLines.Sum(x => x.TotalDebit);
        response.SumTotalCredit = response.TrialBalanceLines.Sum(x => x.TotalCredit);
        response.SumDebitBalance = response.TrialBalanceLines.Sum(x => x.DebitBalance);
        response.SumCreditBalance = response.TrialBalanceLines.Sum(x => x.CreditBalance);

        return response;
    }
}
