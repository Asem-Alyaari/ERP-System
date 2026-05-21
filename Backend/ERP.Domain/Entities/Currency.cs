using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// العملات
/// </summary>
public class Currency : Entity
{
    public string Code { get; private set; } = string.Empty; // e.g., USD, SAR
    public string Name { get; private set; } = string.Empty;
    public string Symbol { get; private set; } = string.Empty;
    public bool IsLocal { get; private set; }

    private readonly List<CurrencyExchangeRate> _exchangeRates = new();
    public virtual IReadOnlyCollection<CurrencyExchangeRate> ExchangeRates => _exchangeRates.AsReadOnly();

    private readonly List<Account> _accounts = new();
    public virtual IReadOnlyCollection<Account> Accounts => _accounts.AsReadOnly();

    private Currency() { } // For EF Core

    public Currency(Guid id, string code, string name, string symbol, bool isLocal) : base(id)
    {
        Code = code;
        Name = name;
        Symbol = symbol;
        IsLocal = isLocal;
    }

    public void Update(string code, string name, string symbol, bool isLocal)
    {
        Code = code;
        Name = name;
        Symbol = symbol;
        IsLocal = isLocal;
    }
}

