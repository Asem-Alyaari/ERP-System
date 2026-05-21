using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Inventory.Items;
using ERP.Application.Features.Inventory.Items.Specifications;
using MediatR;

namespace ERP.Application.Features.Inventory.Items.Queries.GetItemsWithPagination;

public record GetItemsWithPaginationQuery : IRequest<ItemsPagedResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
}

public class ItemsPagedResponse
{
    public List<ItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class GetItemsWithPaginationQueryHandler : IRequestHandler<GetItemsWithPaginationQuery, ItemsPagedResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetItemsWithPaginationQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ItemsPagedResponse> Handle(GetItemsWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;

        var spec = new ItemFilterSpecification(request.SearchTerm, skip, request.PageSize);
        var countSpec = new ItemFilterCountSpecification(request.SearchTerm);

        var items = await _unitOfWork.Repository<Item>().ListAsync(spec);
        var totalCount = await _unitOfWork.Repository<Item>().CountAsync(countSpec);

        return new ItemsPagedResponse
        {
            Items = items.Select(item => new ItemDto
            {
                Id = item.Id,
                ItemCode = item.ItemCode,
                ItemNameAr = item.ItemNameAr,
                ItemNameEn = item.ItemNameEn,
                StockGroupId = item.StockGroupId,
                StockGroupNameAr = item.StockGroup?.GroupNameAr ?? string.Empty,
                StockGroupNameEn = item.StockGroup?.GroupNameEn ?? string.Empty,
                CategoryId = item.CategoryId,
                CategoryNameAr = item.Category?.NameAr,
                CategoryNameEn = item.Category?.NameEn,
                Sku = item.Sku,
                Barcode = item.Barcode,
                DefaultPurchasePrice = item.DefaultPurchasePrice,
                SalesPrice = item.SalesPrice,
                IsActive = item.IsActive,
                ReorderLevel = item.ReorderLevel,
                MinimumQuantity = item.MinimumQuantity,
                MaximumQuantity = item.MaximumQuantity,
                SafetyStockUnitId = item.SafetyStockUnitId,
                ItemUnits = item.ItemUnits.Select(iu => new ItemUnitDto
                {
                    Id = iu.Id,
                    UnitId = iu.UnitId,
                    UnitNameAr = iu.Unit?.NameAr ?? string.Empty,
                    UnitNameEn = iu.Unit?.NameEn ?? string.Empty,
                    ConversionRate = iu.ConversionRate,
                    IsBaseUnit = iu.IsBaseUnit,
                    Price = iu.Price
                }).ToList()
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
