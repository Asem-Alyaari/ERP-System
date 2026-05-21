namespace ERP.Domain.Enums;

public enum AccountType
{
    /// <summary>
    /// الأصول
    /// </summary>
    Asset = 1,

    /// <summary>
    /// الالتزامات (الخصوم)
    /// </summary>
    Liability = 2,

    /// <summary>
    /// حقوق الملكية
    /// </summary>
    Equity = 3,

    /// <summary>
    /// الإيرادات
    /// </summary>
    Revenue = 4,

    /// <summary>
    /// المصروفات
    /// </summary>
    Expense = 5
}
