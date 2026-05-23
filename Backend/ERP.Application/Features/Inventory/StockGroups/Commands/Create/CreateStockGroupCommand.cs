using ERP.Application.Features.Accounting.Currencies.Specifications;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.StockGroups.Commands.Create;

public record CreateStockGroupCommand : IRequest<Guid>
{
    public string GroupCode             { get; init; } = string.Empty;
    public string GroupNameAr           { get; init; } = string.Empty;
    public string GroupNameEn           { get; init; } = string.Empty;
    public bool   IsDetail              { get; init; }
    public Guid?  ParentGroupId         { get; init; }

    // اختيارية — إذا تُركت فارغة يتولّى الـ Handler توليدها أو وراثتها
    public Guid?  InventoryAccountId        { get; init; }
    public Guid?  SalesAccountId            { get; init; }
    public Guid?  CostOfGoodsSoldAccountId  { get; init; }
}

public class CreateStockGroupCommandHandler : IRequestHandler<CreateStockGroupCommand, Guid>
{
    // أكواد الحسابات الجذرية المعرَّفة في DbInitializer — تُستخدم كآباء للحسابات المولّدة تلقائياً
    private const string InventoryParentCode = "1103";
    private const string SalesParentCode     = "4101";
    private const string CogsParentCode      = "5101";

    private readonly IUnitOfWork _unitOfWork;

    public CreateStockGroupCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateStockGroupCommand request, CancellationToken cancellationToken)
    {
        var accountsAreEmpty = !request.InventoryAccountId.HasValue
                            && !request.SalesAccountId.HasValue
                            && !request.CostOfGoodsSoldAccountId.HasValue;

        Guid? inventoryAccountId        = request.InventoryAccountId;
        Guid? salesAccountId            = request.SalesAccountId;
        Guid? costOfGoodsSoldAccountId  = request.CostOfGoodsSoldAccountId;

        if (accountsAreEmpty)
        {
            if (request.ParentGroupId.HasValue)
            {
                // ── سيناريو الوراثة الشجرية ──────────────────────────────────────
                // جلب المجموعة الأب وتطبيق حساباتها كقيم افتراضية
                var parentGroup = await _unitOfWork.Repository<StockGroup>()
                                                   .GetByIdAsync(request.ParentGroupId.Value);

                if (parentGroup is not null)
                {
                    inventoryAccountId       = parentGroup.InventoryAccountId;
                    salesAccountId           = parentGroup.SalesAccountId;
                    costOfGoodsSoldAccountId = parentGroup.CostOfGoodsSoldAccountId;
                }
            }
            else
            {
                // ── سيناريو التوليد التلقائي ──────────────────────────────────────
                // جلب جميع الحسابات دفعة واحدة لتجنب N+1 عند توليد الأكواد
                var allAccounts = await _unitOfWork.Repository<Account>().ListAllAsync();

                // العثور على الحسابات الجذرية بالكود
                var inventoryParent = allAccounts.FirstOrDefault(a => a.AccountCode == InventoryParentCode);
                var salesParent     = allAccounts.FirstOrDefault(a => a.AccountCode == SalesParentCode);
                var cogsParent      = allAccounts.FirstOrDefault(a => a.AccountCode == CogsParentCode);

                // البحث عن العملة الافتراضية (المحلية)
                var localCurrency = await _unitOfWork.Repository<Currency>().GetEntityWithSpec(
                    new LocalCurrencySpecification());
                if (localCurrency is null)
                    throw new InvalidOperationException("لم يتم العثور على العملة المحلية في النظام.");

                // توليد الحسابات الثلاثة إذا وُجد الأب المقابل
                if (inventoryParent is not null)
                {
                    var code = GenerateNextChildCode(allAccounts, inventoryParent.Id, inventoryParent.AccountCode);
                    var account = new Account(
                        Guid.NewGuid(), code,
                        $"مخزن {request.GroupNameAr}",
                        $"Inventory - {request.GroupNameEn}",
                        AccountType.Asset, isDetail: true,
                        localCurrency.Id,
                        inventoryParent.Id);
                    _unitOfWork.Repository<Account>().Add(account);
                    inventoryAccountId = account.Id;
                }

                if (salesParent is not null)
                {
                    var code = GenerateNextChildCode(allAccounts, salesParent.Id, salesParent.AccountCode);
                    var account = new Account(
                        Guid.NewGuid(), code,
                        $"مبيعات {request.GroupNameAr}",
                        $"Sales - {request.GroupNameEn}",
                        AccountType.Revenue, isDetail: true,
                        localCurrency.Id,
                        salesParent.Id);
                    _unitOfWork.Repository<Account>().Add(account);
                    salesAccountId = account.Id;
                }

                if (cogsParent is not null)
                {
                    var code = GenerateNextChildCode(allAccounts, cogsParent.Id, cogsParent.AccountCode);
                    var account = new Account(
                        Guid.NewGuid(), code,
                        $"تكلفة مبيعات {request.GroupNameAr}",
                        $"COGS - {request.GroupNameEn}",
                        AccountType.Expense, isDetail: true,
                        localCurrency.Id,
                        cogsParent.Id);
                    _unitOfWork.Repository<Account>().Add(account);
                    costOfGoodsSoldAccountId = account.Id;
                }

                // حفظ الحسابات الجديدة أولاً قبل ربطها بالمجموعة
                await _unitOfWork.Complete();
            }
        }

        // ─── إنشاء المجموعة بالحسابات المحدَّدة (يدوية أو موروثة أو مولّدة تلقائياً) ───
        var stockGroup = new StockGroup(
            Guid.NewGuid(),
            request.GroupCode,
            request.GroupNameAr,
            request.GroupNameEn,
            request.IsDetail,
            request.ParentGroupId,
            inventoryAccountId,
            salesAccountId,
            costOfGoodsSoldAccountId);

        _unitOfWork.Repository<StockGroup>().Add(stockGroup);
        await _unitOfWork.Complete();

        return stockGroup.Id;
    }

    /// <summary>
    /// يولّد كود الحساب التالي بشكل تسلسلي تحت الأب المحدد.
    /// القاعدة: كود الأب + رقم تسلسلي مكوَّن من رقمين (01, 02, ...).
    /// </summary>
    private static string GenerateNextChildCode(
        IReadOnlyList<Account> allAccounts,
        Guid parentId,
        string parentCode)
    {
        var siblings = allAccounts
            .Where(a => a.ParentAccountId == parentId)
            .ToList();

        if (siblings.Count == 0)
            return parentCode + "01";

        var maxSuffix = siblings
            .Select(s =>
            {
                if (s.AccountCode.StartsWith(parentCode, StringComparison.Ordinal)
                    && int.TryParse(s.AccountCode[parentCode.Length..], out var n))
                    return n;
                return 0;
            })
            .DefaultIfEmpty(0)
            .Max();

        return parentCode + (maxSuffix + 1).ToString("D2");
    }
}
