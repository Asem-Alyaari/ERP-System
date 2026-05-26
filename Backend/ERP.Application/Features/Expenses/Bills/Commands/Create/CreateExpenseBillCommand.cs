using MediatR;
using ERP.Domain.Enums;

namespace ERP.Application.Features.Expenses.Bills.Commands.Create;

public record CreateExpenseBillCommand : IRequest<Guid>
{
    public string BillNumber { get; init; } = string.Empty;
    public string TransactionDate { get; init; } = string.Empty;
    public ExpenseBillPaymentMethod PaymentMethod { get; init; }
    public decimal TotalAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal NetAmount { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public string? VendorId { get; init; }
    public string? SupplierName { get; init; }
    public string? PaymentAccountId { get; init; }
    public List<CreateExpenseBillLineCommand> Lines { get; init; } = new();
}

public record CreateExpenseBillLineCommand
{
    public string AccountId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string CostCenterId { get; init; } = string.Empty;
    public string? Notes { get; init; }
}
