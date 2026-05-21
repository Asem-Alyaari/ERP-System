using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Application.Features.Inventory.Items.Commands.Create; // to reuse ItemUnitCommandDto
using ERP.Application.Features.Inventory.Items.Specifications;
using MediatR;

namespace ERP.Application.Features.Inventory.Items.Commands.Update;

public record UpdateItemCommand : IRequest<MediatR.Unit>
{
    public Guid Id { get; init; }
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
    public bool IsActive { get; init; } = true;

    public List<ItemUnitCommandDto> ItemUnits { get; init; } = new();
}

public class UpdateItemCommandHandler : IRequestHandler<UpdateItemCommand, MediatR.Unit>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateItemCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MediatR.Unit> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
    {
        var spec = new ItemByIdWithDetailsSpecification(request.Id);
        var item = await _unitOfWork.Repository<Item>().GetEntityWithSpec(spec);

        if (item == null)
        {
            throw new Exception($"Item with ID {request.Id} not found");
        }

        item.Update(
            request.ItemCode,
            request.ItemNameAr,
            request.ItemNameEn,
            request.StockGroupId,
            request.Sku,
            request.Barcode,
            request.DefaultPurchasePrice,
            request.SalesPrice
        );

        item.SetCategory(request.CategoryId);

        if (request.SafetyStockUnitId.HasValue)
        {
            item.SetInventoryLimits(request.ReorderLevel, request.MinimumQuantity, request.MaximumQuantity, request.SafetyStockUnitId.Value);
        }

        if (request.IsActive)
        {
            item.Activate();
        }
        else
        {
            item.Deactivate();
        }

        // Clear and add new units
        var unitRepo = _unitOfWork.Repository<ItemUnit>();
        foreach (var u in item.ItemUnits.ToList())
        {
            unitRepo.Delete(u);
        }
        item.ClearUnits();

        if (request.ItemUnits != null)
        {
            foreach (var unit in request.ItemUnits)
            {
                item.AddUnit(unit.UnitId, unit.ConversionRate, unit.IsBaseUnit, unit.Price);
            }
        }

        await _unitOfWork.Complete();

        return MediatR.Unit.Value;
    }
}
