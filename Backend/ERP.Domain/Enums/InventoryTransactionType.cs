namespace ERP.Domain.Enums;

public enum InventoryTransactionType
{
    /// <summary>
    /// إذن إضافة مخزنية
    /// </summary>
    StockIn = 1,

    /// <summary>
    /// إذن صرف مخزني
    /// </summary>
    StockOut = 2,

    /// <summary>
    /// تحويل بين المخازن
    /// </summary>
    Transfer = 3
}
