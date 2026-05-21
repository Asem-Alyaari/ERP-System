using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// تصنيفات الأصناف
/// </summary>
public class Category : Entity
{
    public string NameAr { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;

    private readonly List<Item> _items = new();
    public virtual IReadOnlyCollection<Item> Items => _items.AsReadOnly();

    private Category() { } // For EF Core

    public Category(Guid id, string nameAr, string nameEn) : base(id)
    {
        NameAr = nameAr;
        NameEn = nameEn;
    }

    public void Update(string nameAr, string nameEn)
    {
        NameAr = nameAr;
        NameEn = nameEn;
    }
}
