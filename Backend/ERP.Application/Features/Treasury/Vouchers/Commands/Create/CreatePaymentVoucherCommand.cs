using MediatR;
using ERP.Domain.Enums;

namespace ERP.Application.Features.Treasury.Vouchers.Commands.Create;

public record CreatePaymentVoucherCommand : IRequest<Guid>
{
    public string VoucherNumber { get; init; } = string.Empty;
    public DateTime VoucherDate { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public Guid SourceAccountId { get; init; }
    public VoucherPartnerType DestinationType { get; init; }
    public decimal Amount { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public string? Notes { get; init; }
    public Guid? VendorId { get; init; }
    public Guid? DestinationAccountId { get; init; }
    public Guid? CostCenterId { get; init; }
}
