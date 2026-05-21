namespace ERP.Domain.Enums;

public enum PaymentMethod
{
    /// <summary>
    /// نقدي
    /// </summary>
    Cash = 1,

    /// <summary>
    /// تحويل بنكي
    /// </summary>
    BankTransfer = 2,

    /// <summary>
    /// شيك
    /// </summary>
    Cheque = 3
}
