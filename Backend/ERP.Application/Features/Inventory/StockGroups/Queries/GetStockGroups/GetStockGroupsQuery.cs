using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.StockGroups.Queries.GetStockGroups;

public record GetStockGroupsQuery : IRequest<List<StockGroupDto>>
{
    /// <summary>
    /// إذا كانت true: يُرجع فقط المجموعات التفصيلية (IsDetail == true)
    /// المستخدَمة في القوائم المنسدلة (Dropdown) عند إنشاء الأصناف.
    /// إذا كانت null أو false: يُرجع جميع المجموعات.
    /// </summary>
    public bool? OnlyDetails { get; init; }
}

public class GetStockGroupsQueryHandler : IRequestHandler<GetStockGroupsQuery, List<StockGroupDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetStockGroupsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<StockGroupDto>> Handle(GetStockGroupsQuery request, CancellationToken cancellationToken)
    {
        var stockGroups = await _unitOfWork.Repository<StockGroup>().ListAllAsync();

        var query = stockGroups.AsEnumerable();

        // تصفية المجموعات التفصيلية فقط إذا طُلب ذلك (لخدمة الـ Dropdown)
        if (request.OnlyDetails == true)
            query = query.Where(x => x.IsDetail);

        return query
            .OrderBy(x => x.GroupCode)
            .Select(x => new StockGroupDto
            {
                Id                      = x.Id,
                GroupCode               = x.GroupCode,
                GroupNameAr             = x.GroupNameAr,
                GroupNameEn             = x.GroupNameEn,
                IsDetail                = x.IsDetail,
                ParentGroupId           = x.ParentGroupId,
                InventoryAccountId      = x.InventoryAccountId,
                SalesAccountId          = x.SalesAccountId,
                CostOfGoodsSoldAccountId = x.CostOfGoodsSoldAccountId
            })
            .ToList();
    }
}
