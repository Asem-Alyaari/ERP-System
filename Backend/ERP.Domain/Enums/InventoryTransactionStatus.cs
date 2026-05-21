namespace ERP.Domain.Enums;

public enum InventoryTransactionStatus
{
    /// <summary>
    /// مسودة
    /// </summary>
    Draft = 1,

    /// <summary>
    /// معتمد
    /// </summary>
    Approved = 2,

    /// <summary>
    /// ملغي
    /// </summary>
    Cancelled = 3
}
