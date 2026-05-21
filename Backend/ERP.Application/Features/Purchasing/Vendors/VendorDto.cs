using System;

namespace ERP.Application.Features.Purchasing.Vendors;

public class VendorDto
{
    public Guid Id { get; set; }
    public string VendorCode { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public Guid AccountId { get; set; }
    public string? AccountNameAr { get; set; }
    public string? AccountNameEn { get; set; }
}
