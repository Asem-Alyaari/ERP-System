using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.CostCenters.Queries.GetCostCenters;

/// <summary>
/// استعلام جلب مراكز التكلفة.
/// عند استخدام <c>OnlyDetail = true</c> (الافتراضي)، يُعيد المراكز التفصيلية فقط
/// وهي الأوراق النهائية الصالحة للربط بأسطر القيود المحاسبية.
/// </summary>
public record GetCostCentersQuery : IRequest<List<CostCenterDto>>
{
    /// <summary>
    /// إذا كانت true: يُرجع المراكز التفصيلية فقط (IsDetail == true).
    /// إذا كانت false أو null: يُرجع جميع المراكز.
    /// القيمة الافتراضية: true — مناسبة لحقول الاختيار في نماذج القيود.
    /// </summary>
    public bool? OnlyDetail { get; init; } = true;
}

public class GetCostCentersQueryHandler : IRequestHandler<GetCostCentersQuery, List<CostCenterDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCostCentersQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<CostCenterDto>> Handle(GetCostCentersQuery request, CancellationToken cancellationToken)
    {
        var all = await _unitOfWork.Repository<CostCenter>().ListAllAsync();

        var query = all.AsEnumerable();

        if (request.OnlyDetail == true)
            query = query.Where(x => x.IsDetail);

        return query
            .OrderBy(x => x.CostCenterCode)
            .Select(x => new CostCenterDto
            {
                Id                  = x.Id,
                CostCenterCode      = x.CostCenterCode,
                CostCenterNameAr    = x.CostCenterNameAr,
                CostCenterNameEn    = x.CostCenterNameEn,
                IsDetail            = x.IsDetail,
                ParentCostCenterId  = x.ParentCostCenterId
            })
            .ToList();
    }
}
