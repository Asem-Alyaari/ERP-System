using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// الموردين
/// </summary>
public class Vendor : Entity
{
    public string VendorCode { get; private set; } = string.Empty;
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string? TaxNumber { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }

    // الربط المحاسبي
    public Guid AccountId { get; private set; }
    public virtual Account? Account { get; private set; }

    private readonly List<PurchaseInvoiceMaster> _invoices = new();
    public virtual IReadOnlyCollection<PurchaseInvoiceMaster> Invoices => _invoices.AsReadOnly();

    private Vendor() { } // For EF Core

    public Vendor(
        Guid id, 
        string vendorCode, 
        string nameAr, 
        string nameEn, 
        Guid accountId, 
        string? taxNumber = null, 
        string? phone = null, 
        string? email = null) : base(id)
    {
        VendorCode = vendorCode;
        NameAr = nameAr;
        NameEn = nameEn;
        AccountId = accountId;
        TaxNumber = taxNumber;
        Phone = phone;
        Email = email;
    }

    public void Update(
        string vendorCode,
        string nameAr,
        string nameEn,
        Guid accountId,
        string? taxNumber = null,
        string? phone = null,
        string? email = null)
    {
        VendorCode = vendorCode;
        NameAr = nameAr;
        NameEn = nameEn;
        AccountId = accountId;
        TaxNumber = taxNumber;
        Phone = phone;
        Email = email;
    }
}
