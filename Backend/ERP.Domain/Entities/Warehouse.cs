using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// المخازن والمستودعات
/// </summary>
public class Warehouse : Entity
{
    public string Code { get; private set; } = string.Empty;
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<InventoryTransactionMaster> _inventoryTransactionMasters = new();
    public virtual IReadOnlyCollection<InventoryTransactionMaster> InventoryTransactionMasters => _inventoryTransactionMasters.AsReadOnly();

    private readonly List<InventoryTransactionMaster> _transferToMasters = new();
    public virtual IReadOnlyCollection<InventoryTransactionMaster> TransferToMasters => _transferToMasters.AsReadOnly();

    private Warehouse() { } // For EF Core

    public Warehouse(
        Guid id,
        string code,
        string nameAr,
        string nameEn,
        string? location = null) : base(id)
    {
        Code = code;
        NameAr = nameAr;
        NameEn = nameEn;
        Location = location;
        IsActive = true;
    }

    public void Update(
        string code,
        string nameAr,
        string nameEn,
        string? location = null)
    {
        Code = code;
        NameAr = nameAr;
        NameEn = nameEn;
        Location = location;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}
