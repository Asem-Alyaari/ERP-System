using ERP.Domain.Common;
using ERP.Domain.Enums;

namespace ERP.Domain.Entities;

/// <summary>
/// دليل الحسابات الشجري
/// </summary>
public class Account : Entity
{
    public string AccountCode { get; private set; } = string.Empty;
    public string AccountNameAr { get; private set; } = string.Empty;
    public string AccountNameEn { get; private set; } = string.Empty;
    
    public Guid? ParentAccountId { get; private set; }
    public virtual Account? ParentAccount { get; private set; }
    
    public AccountType AccountType { get; private set; }
    public bool IsDetail { get; private set; }
    
    public Guid CurrencyId { get; private set; }
    public virtual Currency? Currency { get; private set; }

    private readonly List<Account> _subAccounts = new();
    public virtual IReadOnlyCollection<Account> SubAccounts => _subAccounts.AsReadOnly();

    private Account() { } // For EF Core

    public Account(
        Guid id, 
        string accountCode, 
        string accountNameAr, 
        string accountNameEn, 
        AccountType accountType, 
        bool isDetail, 
        Guid currencyId, 
        Guid? parentAccountId = null) : base(id)
    {
        AccountCode = accountCode;
        AccountNameAr = accountNameAr;
        AccountNameEn = accountNameEn;
        AccountType = accountType;
        IsDetail = isDetail;
        CurrencyId = currencyId;
        ParentAccountId = parentAccountId;
    }

    public void UpdateDetails(string nameAr, string nameEn, bool isDetail)
    {
        AccountNameAr = nameAr;
        AccountNameEn = nameEn;
        IsDetail = isDetail;
    }

    public void UpdateDetails(string nameAr, string nameEn, bool isDetail, Guid currencyId)
    {
        UpdateDetails(nameAr, nameEn, isDetail);
        CurrencyId = currencyId;
    }
}
