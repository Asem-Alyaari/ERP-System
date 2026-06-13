namespace ERP.Application.Features.Inventory.Warehouses;

public class WarehouseDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Location { get; set; }
    public bool IsActive { get; set; }
}
