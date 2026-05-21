using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Inventory.Items;
using ERP.Application.Features.Inventory.Items.Specifications;
using MediatR;

namespace ERP.Application.Features.Inventory.Items.Queries.GetItemById;

public record GetItemByIdQuery(Guid Id) : IRequest<ItemDto?>;

public class GetItemByIdQueryHandler : IRequestHandler<GetItemByIdQuery, ItemDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetItemByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ItemDto?> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new ItemByIdWithDetailsSpecification(request.Id);
        var items = await _unitOfWork.Repository<Item>().ListAsync(spec);
        var item = items.FirstOrDefault();

        if (item == null) return null;

        return new ItemDto
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
        };
    }
}
