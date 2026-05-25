using MediatR;
using ERP.Domain.Enums;

namespace ERP.Application.Features.Expenses.Bills.Commands.Create;

public record CreateExpenseBillCommand : IRequest<Guid>
{
    public string BillNumber { get; init; } = string.Empty;
    public DateTime TransactionDate { get; init; }
    public ExpenseBillPaymentMethod PaymentMethod { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal NetAmount { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public Guid? VendorId { get; init; }
    public string? SupplierName { get; init; }
    public Guid? PaymentAccountId { get; init; }
    public List<CreateExpenseBillLineCommand> Lines { get; init; } = new();
}

public record CreateExpenseBillLineCommand
{
    public Guid AccountId { get; init; }
    public decimal Amount { get; init; }
    public Guid CostCenterId { get; init; }
    public string? Notes { get; init; }
}
