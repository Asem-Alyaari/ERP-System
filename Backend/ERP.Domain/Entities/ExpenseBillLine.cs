using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// تفاصيل سطر فاتورة المصروفات
/// </summary>
public class ExpenseBillLine : Entity
{
    public Guid ExpenseBillMasterId { get; private set; }
    public virtual ExpenseBillMaster? ExpenseBillMaster { get; private set; }

    // حساب المصروف (5xxx)
    public Guid AccountId { get; private set; }
    public virtual Account? Account { get; private set; }

    public decimal Amount { get; private set; }

    // مركز التكلفة (إلزامي للمصروفات)
    public Guid CostCenterId { get; private set; }
    public virtual CostCenter? CostCenter { get; private set; }

    public string? Notes { get; private set; }

    private ExpenseBillLine() { } // For EF Core

    public ExpenseBillLine(
        Guid id,
        Guid expenseBillMasterId,
        Guid accountId,
        decimal amount,
        Guid costCenterId,
        string? notes = null) : base(id)
    {
        ExpenseBillMasterId = expenseBillMasterId;
        AccountId = accountId;
        Amount = amount;
        CostCenterId = costCenterId;
        Notes = notes;
    }
}
