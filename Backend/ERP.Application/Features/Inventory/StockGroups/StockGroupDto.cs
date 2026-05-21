namespace ERP.Application.Features.Inventory.StockGroups;

public class StockGroupDto
{
    public Guid Id { get; set; }
    public string GroupCode { get; set; } = string.Empty;
    public string GroupNameAr { get; set; } = string.Empty;
    public string GroupNameEn { get; set; } = string.Empty;
    public Guid? ParentGroupId { get; set; }
    public bool IsDetail { get; set; }
    public Guid? InventoryAccountId { get; set; }
    public Guid? SalesAccountId { get; set; }
    public Guid? CostOfGoodsSoldAccountId { get; set; }
}
