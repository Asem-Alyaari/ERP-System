namespace ERP.Application.Features.Inventory.Units;

public class UnitDto
{
    public Guid Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? ShortName { get; set; }
}
