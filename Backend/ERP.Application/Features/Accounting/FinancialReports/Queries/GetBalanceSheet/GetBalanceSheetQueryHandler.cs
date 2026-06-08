using ERP.Application.Features.Accounting.AccountBalances.Specifications;
using ERP.Application.Features.Accounting.FinancialReports.Queries.GetIncomeStatement;
using ERP.Application.Features.Accounting.Reports.Queries.GetTrialBalance;
using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.FinancialReports.Queries.GetBalanceSheet;

public class GetBalanceSheetQueryHandler : IRequestHandler<GetBalanceSheetQuery, BalanceSheetDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMediator _mediator;

    public GetBalanceSheetQueryHandler(IUnitOfWork unitOfWork, IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _mediator = mediator;
    }

    public async Task<BalanceSheetDto> Handle(GetBalanceSheetQuery request, CancellationToken cancellationToken)
    {
        // 1. جلب الفترة المالية
        var fiscalPeriod = await _unitOfWork.Repository<FiscalPeriod>()
            .GetByIdAsync(request.FiscalPeriodId);

        if (fiscalPeriod == null)
            throw new Exception("الفترة المالية غير موجودة.");

        // 2. جلب صافي الربح/الخسارة من قائمة الدخل
        var incomeStatement = await _mediator.Send(new GetIncomeStatementQuery(
            request.FiscalPeriodId,
            fiscalPeriod.StartDate,
            request.AsOfDate ?? fiscalPeriod.EndDate,
            request.CostCenterId
        ));

        var netProfitLoss = incomeStatement.NetProfitLoss;

        // 3. جلب جميع الأرصدة للفترة المالية
        var balanceSpec = new TrialBalanceSpecification(
            request.FiscalPeriodId,
            request.CostCenterId,
            null
        );

        var balances = await _unitOfWork.Repository<AccountBalance>()
            .ListAsync(balanceSpec);

        // 4. جلب جميع الحسابات
        var accounts = await _unitOfWork.Repository<Account>()
            .ListAllAsync();

        // 5. تصفية الحسابات: الأصول (تبدأ بـ 1)، الخصوم (تبدأ بـ 2)، حقوق الملكية (تبدأ بـ 3)
        var assetAccounts = accounts.Where(a => a.AccountCode.StartsWith("1")).ToList();
        var liabilityAccounts = accounts.Where(a => a.AccountCode.StartsWith("2")).ToList();
        var equityAccounts = accounts.Where(a => a.AccountCode.StartsWith("3")).ToList();

        // 6. تجميع الأرصدة لكل حساب
        var assetLines = new List<BalanceSheetLineDto>();
        var liabilityLines = new List<BalanceSheetLineDto>();
        var equityLines = new List<BalanceSheetLineDto>();

        foreach (var account in assetAccounts)
        {
            var accountBalances = balances.Where(b => b.AccountId == account.Id).ToList();
            var totalBalance = accountBalances.Sum(b => b.CurrentBalance);

            assetLines.Add(new BalanceSheetLineDto
            {
                AccountId = account.Id,
                AccountCode = account.AccountCode,
                AccountNameAr = account.AccountNameAr,
                AccountNameEn = account.AccountNameEn,
                Balance = totalBalance,
                Level = account.AccountCode.Length,
                ParentAccountId = account.ParentAccountId
            });
        }

        foreach (var account in liabilityAccounts)
        {
            var accountBalances = balances.Where(b => b.AccountId == account.Id).ToList();
            var totalBalance = accountBalances.Sum(b => b.CurrentBalance);

            liabilityLines.Add(new BalanceSheetLineDto
            {
                AccountId = account.Id,
                AccountCode = account.AccountCode,
                AccountNameAr = account.AccountNameAr,
                AccountNameEn = account.AccountNameEn,
                Balance = totalBalance,
                Level = account.AccountCode.Length,
                ParentAccountId = account.ParentAccountId
            });
        }

        foreach (var account in equityAccounts)
        {
            var accountBalances = balances.Where(b => b.AccountId == account.Id).ToList();
            var totalBalance = accountBalances.Sum(b => b.CurrentBalance);

            equityLines.Add(new BalanceSheetLineDto
            {
                AccountId = account.Id,
                AccountCode = account.AccountCode,
                AccountNameAr = account.AccountNameAr,
                AccountNameEn = account.AccountNameEn,
                Balance = totalBalance,
                Level = account.AccountCode.Length,
                ParentAccountId = account.ParentAccountId
            });
        }

        // 7. إضافة صافي الربح/الخسارة إلى حقوق الملكية (الأرباح المحتجزة)
        equityLines.Add(new BalanceSheetLineDto
        {
            AccountId = Guid.Empty,
            AccountCode = "9999",
            AccountNameAr = "صافي الربح/الخسارة للفترة",
            AccountNameEn = "Net Profit/Loss for Period",
            Balance = netProfitLoss,
            Level = 4,
            ParentAccountId = null
        });

        // 8. حساب المجاميع
        var totalAssets = assetLines.Sum(a => a.Balance);
        var totalLiabilities = liabilityLines.Sum(l => l.Balance);
        var totalEquity = equityLines.Sum(e => e.Balance);

        // 9. التحقق من التوازن
        var isBalanced = Math.Abs(totalAssets - (totalLiabilities + totalEquity)) < 0.01m;

        return new BalanceSheetDto
        {
            PeriodName = fiscalPeriod.YearName,
            AsOfDate = request.AsOfDate ?? fiscalPeriod.EndDate,
            Assets = assetLines.OrderBy(a => a.AccountCode).ToList(),
            Liabilities = liabilityLines.OrderBy(l => l.AccountCode).ToList(),
            Equity = equityLines.OrderBy(e => e.AccountCode).ToList(),
            TotalAssets = totalAssets,
            TotalLiabilities = totalLiabilities,
            TotalEquity = totalEquity,
            NetProfitLoss = netProfitLoss,
            IsBalanced = isBalanced
        };
    }
}
