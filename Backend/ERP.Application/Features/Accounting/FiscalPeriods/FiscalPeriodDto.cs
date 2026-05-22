namespace ERP.Application.Features.Accounting.FiscalPeriods;

public class FiscalPeriodDto
{
    public Guid Id { get; set; }
    public string YearName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsClosed { get; set; }
}
