using ERP.Domain.Entities;

namespace ERP.Domain.Services;

/// <summary>
/// خدمة مجال لإدارة تحديثات الأرصدة المحاسبية بشكل مركزي وآمن
/// </summary>
public interface IAccountBalanceService
{
    /// <summary>
    /// تحديث الأرصدة التراكمية لسطر قيد محاسبي واحد
    /// </summary>
    Task UpdateBalanceForLineAsync(
        JournalEntryLine line,
        Guid fiscalPeriodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// تحديث الأرصدة التراكمية لمجموعة أسطر قيد محاسبي
    /// </summary>
    Task UpdateBalancesForLinesAsync(
        IEnumerable<JournalEntryLine> lines,
        Guid fiscalPeriodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// عكس تأثير سطر قيد محاسبي على الأرصدة (لإلغاء الترحيل)
    /// </summary>
    Task ReverseBalanceForLineAsync(
        JournalEntryLine line,
        Guid fiscalPeriodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// عكس تأثير مجموعة أسطر قيد محاسبي على الأرصدة
    /// </summary>
    Task ReverseBalancesForLinesAsync(
        IEnumerable<JournalEntryLine> lines,
        Guid fiscalPeriodId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// تعيين رصيد افتتاحي لحساب في فترة مالية
    /// </summary>
    Task SetOpeningBalanceAsync(
        Guid accountId,
        Guid fiscalPeriodId,
        decimal debit,
        decimal credit,
        Guid? costCenterId = null,
        Guid? currencyId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// التحقق من توازن مجموعة أرصدة افتتاحية
    /// </summary>
    (bool IsBalanced, decimal TotalDebit, decimal TotalCredit) ValidateOpeningBalances(
        IEnumerable<(Guid AccountId, decimal Debit, decimal Credit, Guid? CostCenterId, Guid? CurrencyId)> balances);
}
