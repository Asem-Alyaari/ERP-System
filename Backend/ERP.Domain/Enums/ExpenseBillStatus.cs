namespace ERP.Domain.Enums;

public enum ExpenseBillStatus
{
    /// <summary>
    /// مسودة
    /// </summary>
    Draft = 1,

    /// <summary>
    /// مرحل (تم إثبات الأثر المالي)
    /// </summary>
    Posted = 2,

    /// <summary>
    /// ملغي
    /// </summary>
    Cancelled = 3
}
