using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Items.Commands.Create;

public record CreateItemCommand : IRequest<Guid>
{
    public string ItemCode { get; init; } = string.Empty;
    public string ItemNameAr { get; init; } = string.Empty;
    public string ItemNameEn { get; init; } = string.Empty;
    public Guid StockGroupId { get; init; }
    public Guid? CategoryId { get; init; }
    public string? Sku { get; init; }
    public string? Barcode { get; init; }
    public decimal DefaultPurchasePrice { get; init; }
    public decimal SalesPrice { get; init; }

    // Inventory limits
    public decimal ReorderLevel { get; init; }
    public decimal MinimumQuantity { get; init; }
    public decimal MaximumQuantity { get; init; }
    public Guid? SafetyStockUnitId { get; init; }

    public List<ItemUnitCommandDto> ItemUnits { get; init; } = new();
}

public class ItemUnitCommandDto
{
    public Guid UnitId { get; set; }
    public decimal ConversionRate { get; set; }
    public bool IsBaseUnit { get; set; }
    public decimal Price { get; set; }
}

public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateItemCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        var item = new Item(
            Guid.NewGuid(),
            request.ItemCode,
            request.ItemNameAr,
            request.ItemNameEn,
            request.StockGroupId,
            request.Sku,
            request.Barcode,
            request.DefaultPurchasePrice,
            request.SalesPrice
        );

        if (request.CategoryId.HasValue)
        {
            item.SetCategory(request.CategoryId.Value);
        }

        if (request.SafetyStockUnitId.HasValue)
        {
            item.SetInventoryLimits(request.ReorderLevel, request.MinimumQuantity, request.MaximumQuantity, request.SafetyStockUnitId.Value);
        }

        if (request.ItemUnits != null)
        {
            foreach (var unit in request.ItemUnits)
            {
                item.AddUnit(unit.UnitId, unit.ConversionRate, unit.IsBaseUnit, unit.Price);
            }
        }

        _unitOfWork.Repository<Item>().Add(item);
        await _unitOfWork.Complete();

        return item.Id;
    }
}
