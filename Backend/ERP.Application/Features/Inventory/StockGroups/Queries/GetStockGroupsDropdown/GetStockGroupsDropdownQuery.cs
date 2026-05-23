using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.StockGroups.Queries.GetStockGroupsDropdown;

/// <summary>
/// DTO مخصص لقوائم الاختيار المنسدلة (Dropdown) في شاشة إنشاء/تعديل الأصناف.
/// يحمل الحد الأدنى من البيانات المطلوبة لعرض الاختيار وإتمام الربط المحاسبي.
/// </summary>
public class StockGroupDropdownDto
{
    public Guid Id { get; set; }
    public string GroupCode { get; set; } = string.Empty;
    public string GroupNameAr { get; set; } = string.Empty;
    public string GroupNameEn { get; set; } = string.Empty;

    /// <summary>
    /// الحساب الافتراضي للمخزون الموروث من هذه المجموعة.
    /// يُستخدم لعرض الحساب في واجهة المستخدم قبل حفظ الصنف.
    /// </summary>
    public Guid? InventoryAccountId { get; set; }

    /// <summary>حساب المبيعات الموروث من هذه المجموعة.</summary>
    public Guid? SalesAccountId { get; set; }

    /// <summary>حساب تكلفة البضاعة المباعة الموروث من هذه المجموعة.</summary>
    public Guid? CostOfGoodsSoldAccountId { get; set; }
}

/// <summary>
/// استعلام جلب مجموعات الأصناف التفصيلية فقط لتغذية قوائم الاختيار
/// في شاشات إنشاء وتعديل الأصناف/المنتجات.
/// يُعيد المجموعات ذات IsDetail == true مرتبةً حسب الكود.
/// </summary>
public record GetStockGroupsDropdownQuery : IRequest<List<StockGroupDropdownDto>>;

public class GetStockGroupsDropdownQueryHandler
    : IRequestHandler<GetStockGroupsDropdownQuery, List<StockGroupDropdownDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetStockGroupsDropdownQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<StockGroupDropdownDto>> Handle(
        GetStockGroupsDropdownQuery request,
        CancellationToken cancellationToken)
    {
        var stockGroups = await _unitOfWork.Repository<StockGroup>().ListAllAsync();

        return stockGroups
            .Where(x => x.IsDetail)
            .OrderBy(x => x.GroupCode)
            .Select(x => new StockGroupDropdownDto
            {
                Id                       = x.Id,
                GroupCode                = x.GroupCode,
                GroupNameAr              = x.GroupNameAr,
                GroupNameEn              = x.GroupNameEn,
                InventoryAccountId       = x.InventoryAccountId,
                SalesAccountId           = x.SalesAccountId,
                CostOfGoodsSoldAccountId = x.CostOfGoodsSoldAccountId
            })
            .ToList();
    }
}
