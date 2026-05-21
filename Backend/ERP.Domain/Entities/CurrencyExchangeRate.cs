using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// أسعار الصرف للعملات
/// </summary>
public class CurrencyExchangeRate : Entity
{
    public Guid CurrencyId { get; private set; }
    public decimal Rate { get; private set; }
    public DateTime EffectiveDate { get; private set; }

    public virtual Currency? Currency { get; private set; }

    private CurrencyExchangeRate() { } // For EF Core

    public CurrencyExchangeRate(Guid id, Guid currencyId, decimal rate, DateTime effectiveDate) : base(id)
    {
        CurrencyId = currencyId;
        Rate = rate;
        EffectiveDate = effectiveDate;
    }

    public void Update(Guid currencyId, decimal rate, DateTime effectiveDate)
    {
        CurrencyId = currencyId;
        Rate = rate;
        EffectiveDate = effectiveDate;
    }
}

