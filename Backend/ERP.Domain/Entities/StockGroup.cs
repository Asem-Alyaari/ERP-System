using ERP.Domain.Common;

namespace ERP.Domain.Entities;

/// <summary>
/// مجموعات الأصناف
/// </summary>
public class StockGroup : Entity
{
    public string GroupCode { get; private set; } = string.Empty;
    public string GroupNameAr { get; private set; } = string.Empty;
    public string GroupNameEn { get; private set; } = string.Empty;

    public Guid? ParentGroupId { get; private set; }
    public virtual StockGroup? ParentGroup { get; private set; }

    public bool IsDetail { get; private set; }

    // الربط المحاسبي
    public Guid? InventoryAccountId { get; private set; }
    public virtual Account? InventoryAccount { get; private set; }

    public Guid? SalesAccountId { get; private set; }
    public virtual Account? SalesAccount { get; private set; }

    public Guid? CostOfGoodsSoldAccountId { get; private set; }
    public virtual Account? CostOfGoodsSoldAccount { get; private set; }

    private readonly List<StockGroup> _subGroups = new();
    public virtual IReadOnlyCollection<StockGroup> SubGroups => _subGroups.AsReadOnly();

    private readonly List<Item> _items = new();
    public virtual IReadOnlyCollection<Item> Items => _items.AsReadOnly();

    private StockGroup() { } // For EF Core

    public StockGroup(
        Guid id, 
        string groupCode, 
        string groupNameAr, 
        string groupNameEn, 
        bool isDetail, 
        Guid? parentGroupId = null,
        Guid? inventoryAccountId = null,
        Guid? salesAccountId = null,
        Guid? cogsAccountId = null) : base(id)
    {
        GroupCode = groupCode;
        GroupNameAr = groupNameAr;
        GroupNameEn = groupNameEn;
        IsDetail = isDetail;
        ParentGroupId = parentGroupId;
        InventoryAccountId = inventoryAccountId;
        SalesAccountId = salesAccountId;
        CostOfGoodsSoldAccountId = cogsAccountId;
    }

    public void Update(
        string groupCode,
        string groupNameAr,
        string groupNameEn,
        bool isDetail,
        Guid? parentGroupId = null,
        Guid? inventoryAccountId = null,
        Guid? salesAccountId = null,
        Guid? cogsAccountId = null)
    {
        GroupCode = groupCode;
        GroupNameAr = groupNameAr;
        GroupNameEn = groupNameEn;
        IsDetail = isDetail;
        ParentGroupId = parentGroupId;
        InventoryAccountId = inventoryAccountId;
        SalesAccountId = salesAccountId;
        CostOfGoodsSoldAccountId = cogsAccountId;
    }
}

