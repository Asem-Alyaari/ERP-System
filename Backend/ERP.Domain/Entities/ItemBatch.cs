using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// دفعات الأصناف وتتبع تواريخ الانتهاء
/// </summary>
public class ItemBatch : Entity
{
    public Guid ItemId { get; private set; }
    public virtual Item? Item { get; private set; }

    public string BatchNumber { get; private set; } = string.Empty;
    public DateTime? ProductionDate { get; private set; }
    public DateTime? ExpiryDate { get; private set; }

    public decimal PurchasePrice { get; private set; } // سعر الشراء الفعلي لهذه الدفعة
    public decimal QuantityOnHand { get; private set; } // الكمية المتبقية حالياً في هذه الدفعة

    private ItemBatch() { } // For EF Core

    public ItemBatch(
        Guid id, 
        Guid itemId, 
        string batchNumber, 
        decimal purchasePrice, 
        decimal quantityOnHand, 
        DateTime? productionDate = null, 
        DateTime? expiryDate = null) : base(id)
    {
        ItemId = itemId;
        BatchNumber = batchNumber;
        PurchasePrice = purchasePrice;
        QuantityOnHand = quantityOnHand;
        ProductionDate = productionDate;
        ExpiryDate = expiryDate;
    }

    public void UpdateQuantity(decimal change)
    {
        QuantityOnHand += change;
    }
}
