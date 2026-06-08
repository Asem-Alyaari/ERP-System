using MediatR;

namespace ERP.Application.Features.Accounting.FinancialReports.Queries.GetIncomeStatement;

public record GetIncomeStatementQuery(
    Guid FiscalPeriodId,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    Guid? CostCenterId = null
) : IRequest<IncomeStatementDto>;

public record IncomeStatementDto
{
    public string PeriodName { get; init; } = string.Empty;
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public List<IncomeStatementLineDto> Revenues { get; init; } = new();
    public List<IncomeStatementLineDto> Expenses { get; init; } = new();
    public decimal TotalRevenues { get; init; }
    public decimal TotalExpenses { get; init; }
    public decimal NetProfitLoss { get; init; }
}

public record IncomeStatementLineDto
{
    public Guid AccountId { get; init; }
    public string AccountCode { get; init; } = string.Empty;
    public string AccountNameAr { get; init; } = string.Empty;
    public string AccountNameEn { get; init; } = string.Empty;
    public decimal Balance { get; init; }
    public int Level { get; init; }
    public Guid? ParentAccountId { get; init; }
}
