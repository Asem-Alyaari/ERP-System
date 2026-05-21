namespace ERP.Domain.Enums;

public enum PurchaseInvoiceStatus
{
    /// <summary>
    /// مسودة
    /// </summary>
    Draft = 1,

    /// <summary>
    /// مرحلة (معتمدة ماليًا ومخزنيًا)
    /// </summary>
    Posted = 2,

    /// <summary>
    /// ملغاة
    /// </summary>
    Cancelled = 3
}
