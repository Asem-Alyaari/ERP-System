using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// الفترات المالية
/// </summary>
public class FiscalPeriod : Entity
{
    public string YearName { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsClosed { get; private set; }

    private FiscalPeriod() { } // For EF Core

    public FiscalPeriod(Guid id, string yearName, DateTime startDate, DateTime endDate) : base(id)
    {
        YearName = yearName;
        StartDate = startDate;
        EndDate = endDate;
        IsClosed = false;
    }

    public void ClosePeriod()
    {
        IsClosed = true;
    }

    public void OpenPeriod()
    {
        IsClosed = false;
    }
}
