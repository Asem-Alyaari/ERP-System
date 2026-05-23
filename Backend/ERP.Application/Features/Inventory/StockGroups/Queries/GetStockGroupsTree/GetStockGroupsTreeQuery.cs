using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.StockGroups.Queries.GetStockGroupsTree;

/// <summary>
/// DTO الشجري الذي يحمل قائمة الأبناء بشكل تكراري (Recursive).
/// يُستخدم في شاشة تهيئة النظام لعرض الهيكل الكامل لمجموعات الأصناف.
/// </summary>
public class StockGroupTreeDto
{
    public Guid    Id                       { get; set; }
    public string  GroupCode                { get; set; } = string.Empty;
    public string  GroupNameAr              { get; set; } = string.Empty;
    public string  GroupNameEn              { get; set; } = string.Empty;
    public bool    IsDetail                 { get; set; }
    public Guid?   ParentGroupId            { get; set; }
    public Guid?   InventoryAccountId       { get; set; }
    public Guid?   SalesAccountId           { get; set; }
    public Guid?   CostOfGoodsSoldAccountId { get; set; }

    /// <summary>المجموعات الفرعية المتفرعة من هذه المجموعة.</summary>
    public List<StockGroupTreeDto> SubGroups { get; set; } = new();
}

public record GetStockGroupsTreeQuery : IRequest<List<StockGroupTreeDto>>;

public class GetStockGroupsTreeQueryHandler : IRequestHandler<GetStockGroupsTreeQuery, List<StockGroupTreeDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetStockGroupsTreeQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<StockGroupTreeDto>> Handle(
        GetStockGroupsTreeQuery request,
        CancellationToken cancellationToken)
    {
        // جلب جميع المجموعات دفعة واحدة لتجنب N+1 queries
        var allGroups = await _unitOfWork.Repository<StockGroup>().ListAllAsync();

        // بناء خريطة Id → DTO لربط الأبناء بآبائهم بكفاءة O(n)
        var dtoMap = allGroups.ToDictionary(
            g => g.Id,
            g => new StockGroupTreeDto
            {
                Id                       = g.Id,
                GroupCode                = g.GroupCode,
                GroupNameAr              = g.GroupNameAr,
                GroupNameEn              = g.GroupNameEn,
                IsDetail                 = g.IsDetail,
                ParentGroupId            = g.ParentGroupId,
                InventoryAccountId       = g.InventoryAccountId,
                SalesAccountId           = g.SalesAccountId,
                CostOfGoodsSoldAccountId = g.CostOfGoodsSoldAccountId
            });

        // بناء الشجرة: إلحاق كل عنصر بقائمة SubGroups الخاصة بأبيه
        var roots = new List<StockGroupTreeDto>();

        foreach (var dto in dtoMap.Values)
        {
            if (dto.ParentGroupId.HasValue && dtoMap.TryGetValue(dto.ParentGroupId.Value, out var parent))
                parent.SubGroups.Add(dto);
            else
                roots.Add(dto);
        }

        // ترتيب تصاعدي بالكود في جميع مستويات الشجرة
        SortTreeRecursively(roots);

        return roots;
    }

    private static void SortTreeRecursively(List<StockGroupTreeDto> nodes)
    {
        nodes.Sort((a, b) => string.Compare(a.GroupCode, b.GroupCode, StringComparison.Ordinal));
        foreach (var node in nodes)
            SortTreeRecursively(node.SubGroups);
    }
}
