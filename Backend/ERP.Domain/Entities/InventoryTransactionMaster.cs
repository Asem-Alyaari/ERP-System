using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

/// <summary>
/// رأس الحركة المخزنية (إذن إضافة، إذن صرف، تحويل)
/// </summary>
public class InventoryTransactionMaster : Entity
{
    public string DocumentNumber { get; private set; } = string.Empty;
    public DateTime TransactionDate { get; private set; }
    public InventoryTransactionType TransactionType { get; private set; }
    public string? Notes { get; private set; }
    public InventoryTransactionStatus Status { get; private set; }

    // Warehouse fields
    public Guid WarehouseId { get; private set; }
    public virtual Warehouse? Warehouse { get; private set; }

    public Guid? ToWarehouseId { get; private set; } // For transfers only
    public virtual Warehouse? ToWarehouse { get; private set; }

    public bool IsPosted { get; private set; } // Accounting integration flag

    // Audit Fields
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public string? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }

    private readonly List<InventoryTransactionLine> _lines = new();
    public virtual IReadOnlyCollection<InventoryTransactionLine> Lines => _lines.AsReadOnly();

    private InventoryTransactionMaster() { } // For EF Core

    public InventoryTransactionMaster(
        Guid id,
        string documentNumber,
        DateTime transactionDate,
        InventoryTransactionType transactionType,
        Guid warehouseId,
        string createdBy,
        string? notes = null,
        Guid? toWarehouseId = null) : base(id)
    {
        DocumentNumber = documentNumber;
        TransactionDate = transactionDate;
        TransactionType = transactionType;
        WarehouseId = warehouseId;
        ToWarehouseId = toWarehouseId;
        CreatedBy = createdBy;
        CreatedAt = DateTime.UtcNow;
        Notes = notes;
        Status = InventoryTransactionStatus.Draft;
        IsPosted = false;
    }

    public void Approve(string approvedBy)
    {
        Status = InventoryTransactionStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = InventoryTransactionStatus.Cancelled;
    }

    public void MarkAsPosted()
    {
        IsPosted = true;
    }

    public void SetWarehouse(Guid warehouseId, Guid? toWarehouseId = null)
    {
        WarehouseId = warehouseId;
        ToWarehouseId = toWarehouseId;
    }
}
