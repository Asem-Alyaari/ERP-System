using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// مراكز التكلفة
/// </summary>
public class CostCenter : Entity
{
    public string CostCenterCode { get; private set; } = string.Empty;
    public string CostCenterNameAr { get; private set; } = string.Empty;
    public string CostCenterNameEn { get; private set; } = string.Empty;

    public Guid? ParentCostCenterId { get; private set; }
    public virtual CostCenter? ParentCostCenter { get; private set; }

    public bool IsDetail { get; private set; }

    private readonly List<CostCenter> _subCostCenters = new();
    public virtual IReadOnlyCollection<CostCenter> SubCostCenters => _subCostCenters.AsReadOnly();

    private readonly List<JournalEntryLine> _journalEntryLines = new();
    public virtual IReadOnlyCollection<JournalEntryLine> JournalEntryLines => _journalEntryLines.AsReadOnly();

    private CostCenter() { } // For EF Core

    public CostCenter(
        Guid id, 
        string costCenterCode, 
        string costCenterNameAr, 
        string costCenterNameEn, 
        bool isDetail, 
        Guid? parentCostCenterId = null) : base(id)
    {
        CostCenterCode = costCenterCode;
        CostCenterNameAr = costCenterNameAr;
        CostCenterNameEn = costCenterNameEn;
        IsDetail = isDetail;
        ParentCostCenterId = parentCostCenterId;
    }
}
