using ERP.Domain.Enums;

namespace ERP.Application.Features.Treasury.Vouchers.DTOs;

public class PaymentVoucherListItemDto
{
    public Guid Id { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public DateTime VoucherDate { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public Guid SourceAccountId { get; set; }
    public string? SourceAccountCode { get; set; }
    public string? SourceAccountNameAr { get; set; }
    public VoucherPartnerType DestinationType { get; set; }
    public Guid? VendorId { get; set; }
    public string? VendorName { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? DestinationAccountId { get; set; }
    public string? DestinationAccountCode { get; set; }
    public string? DestinationAccountNameAr { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public VoucherStatus Status { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? PostedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }
    public string? CostCenterNameAr { get; set; }
}
