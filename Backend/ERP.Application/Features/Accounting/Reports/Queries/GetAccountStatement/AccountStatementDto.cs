namespace ERP.Application.Features.Accounting.Reports.Queries.GetAccountStatement;

public class AccountStatementDto
{
    public decimal OpeningBalance { get; set; }
    public List<AccountStatementLineDto> StatementLines { get; set; } = new();
    public decimal ClosingBalance { get; set; }
}

public class AccountStatementLineDto
{
    public DateTime TransactionDate { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
}
