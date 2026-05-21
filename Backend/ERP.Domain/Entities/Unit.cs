using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// الوحدات العامة (حبة، كرتون، باكت، إلخ)
/// </summary>
public class Unit : Entity
{
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string? ShortName { get; private set; }

    private Unit() { } // For EF Core

    public Unit(Guid id, string nameAr, string nameEn, string? shortName = null) : base(id)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        ShortName = shortName;
    }

    public void Update(string nameAr, string nameEn, string? shortName = null)
    {
        NameAr = nameAr;
        NameEn = nameEn;
        ShortName = shortName;
    }
}
