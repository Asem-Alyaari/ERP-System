namespace ERP.Application.Features.Accounting.CostCenters;

/// <summary>
/// DTO لبيانات مركز التكلفة المُعادة إلى طبقة API.
/// </summary>
public class CostCenterDto
{
    public Guid Id { get; set; }
    public string CostCenterCode { get; set; } = string.Empty;
    public string CostCenterNameAr { get; set; } = string.Empty;
    public string CostCenterNameEn { get; set; } = string.Empty;
    public bool IsDetail { get; set; }
    public Guid? ParentCostCenterId { get; set; }
}
