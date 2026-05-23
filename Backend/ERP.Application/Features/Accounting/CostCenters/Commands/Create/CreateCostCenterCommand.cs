using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Accounting.CostCenters.Commands.Create;

/// <summary>
/// أمر إنشاء مركز تكلفة جديد.
/// يدعم إنشاء مراكز شجرية (Parent) ومراكز تفصيلية (Detail/Leaf).
/// </summary>
public record CreateCostCenterCommand : IRequest<Guid>
{
    /// <summary>الكود الفريد لمركز التكلفة.</summary>
    public string CostCenterCode { get; init; } = string.Empty;

    /// <summary>اسم مركز التكلفة بالعربية.</summary>
    public string CostCenterNameAr { get; init; } = string.Empty;

    /// <summary>اسم مركز التكلفة بالإنجليزية.</summary>
    public string CostCenterNameEn { get; init; } = string.Empty;

    /// <summary>
    /// هل هذا المركز تفصيلي (Leaf)؟
    /// المراكز التفصيلية فقط تظهر في قوائم اختيار القيود.
    /// </summary>
    public bool IsDetail { get; init; } = true;

    /// <summary>معرّف مركز التكلفة الأب (اختياري — null إذا كان جذر الشجرة).</summary>
    public Guid? ParentCostCenterId { get; init; }
}

public class CreateCostCenterCommandHandler : IRequestHandler<CreateCostCenterCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCostCenterCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCostCenterCommand request, CancellationToken cancellationToken)
    {
        // ── التحقق من عدم تكرار الكود ──────────────────────────────────────────
        var existingCodes = (await _unitOfWork.Repository<CostCenter>().ListAllAsync())
            .Select(c => c.CostCenterCode)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (existingCodes.Contains(request.CostCenterCode))
            throw new InvalidOperationException(
                $"كود مركز التكلفة '{request.CostCenterCode}' مستخدم مسبقاً. يرجى اختيار كود فريد.");

        // ── التحقق من صحة مركز التكلفة الأب (إن وُجد) ────────────────────────
        if (request.ParentCostCenterId.HasValue)
        {
            var parent = await _unitOfWork.Repository<CostCenter>()
                .GetByIdAsync(request.ParentCostCenterId.Value);

            if (parent is null)
                throw new InvalidOperationException(
                    $"مركز التكلفة الأب بالمعرّف '{request.ParentCostCenterId}' غير موجود.");

            // لا يمكن ربط مركز أب تفصيلي بمراكز أبناء (المراكز التفصيلية هي أوراق الشجرة)
            if (parent.IsDetail)
                throw new InvalidOperationException(
                    "لا يمكن إضافة مركز تكلفة تحت مركز تفصيلي. المراكز التفصيلية هي الأوراق النهائية للشجرة.");
        }

        // ── إنشاء مركز التكلفة ────────────────────────────────────────────────
        var costCenter = new CostCenter(
            Guid.NewGuid(),
            request.CostCenterCode,
            request.CostCenterNameAr,
            request.CostCenterNameEn,
            request.IsDetail,
            request.ParentCostCenterId);

        _unitOfWork.Repository<CostCenter>().Add(costCenter);
        await _unitOfWork.Complete();

        return costCenter.Id;
    }
}
