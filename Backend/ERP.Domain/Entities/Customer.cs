using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// العملاء
/// </summary>
public class Customer : Entity
{
    public string CustomerCode { get; private set; } = string.Empty;
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string? TaxNumber { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }

    // الربط المحاسبي
    public Guid AccountId { get; private set; }
    public virtual Account? Account { get; private set; }

    private readonly List<SalesInvoiceMaster> _invoices = new();
    public virtual IReadOnlyCollection<SalesInvoiceMaster> Invoices => _invoices.AsReadOnly();

    private Customer() { } // For EF Core

    public Customer(
        Guid id, 
        string customerCode, 
        string nameAr, 
        string nameEn, 
        Guid accountId, 
        string? taxNumber = null, 
        string? phone = null, 
        string? email = null) : base(id)
    {
        CustomerCode = customerCode;
        NameAr = nameAr;
        NameEn = nameEn;
        AccountId = accountId;
        TaxNumber = taxNumber;
        Phone = phone;
        Email = email;
    }

    public void Update(
        string customerCode,
        string nameAr,
        string nameEn,
        Guid accountId,
        string? taxNumber = null,
        string? phone = null,
        string? email = null)
    {
        CustomerCode = customerCode;
        NameAr = nameAr;
        NameEn = nameEn;
        AccountId = accountId;
        TaxNumber = taxNumber;
        Phone = phone;
        Email = email;
    }
}
