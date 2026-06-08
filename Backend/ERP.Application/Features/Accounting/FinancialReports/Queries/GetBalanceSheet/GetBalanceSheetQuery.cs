using MediatR;

namespace ERP.Application.Features.Accounting.FinancialReports.Queries.GetBalanceSheet;

public record GetBalanceSheetQuery(
    Guid FiscalPeriodId,
    DateTime? AsOfDate = null,
    Guid? CostCenterId = null
) : IRequest<BalanceSheetDto>;

public record BalanceSheetDto
{
    public string PeriodName { get; init; } = string.Empty;
    public DateTime AsOfDate { get; init; }
    public List<BalanceSheetLineDto> Assets { get; init; } = new();
    public List<BalanceSheetLineDto> Liabilities { get; init; } = new();
    public List<BalanceSheetLineDto> Equity { get; init; } = new();
    public decimal TotalAssets { get; init; }
    public decimal TotalLiabilities { get; init; }
    public decimal TotalEquity { get; init; }
    public decimal NetProfitLoss { get; init; }
    public bool IsBalanced { get; init; }
}

public record BalanceSheetLineDto
{
    public Guid AccountId { get; init; }
    public string AccountCode { get; init; } = string.Empty;
    public string AccountNameAr { get; init; } = string.Empty;
    public string AccountNameEn { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public int Level { get; init; }
    public Guid? ParentAccountId { get; init; }
}
