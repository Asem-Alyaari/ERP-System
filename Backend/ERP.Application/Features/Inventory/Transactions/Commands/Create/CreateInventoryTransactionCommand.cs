using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Repositories;
using MediatR;

namespace ERP.Application.Features.Inventory.Transactions.Commands.Create;

public record CreateInventoryTransactionCommand : IRequest<Guid>
{
    public string DocumentNumber { get; init; } = string.Empty;
    public DateTime TransactionDate { get; init; }
    public InventoryTransactionType TransactionType { get; init; }
    public Guid WarehouseId { get; init; }
    public Guid? ToWarehouseId { get; init; } // For transfers
    public string? Notes { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public List<InventoryTransactionLineDto> Lines { get; init; } = new();
}

public record InventoryTransactionLineDto
{
    public Guid ItemId { get; init; }
    public Guid UnitId { get; init; }
    public decimal Quantity { get; init; }
    public decimal ConversionRate { get; init; }
    public decimal Price { get; init; }
    public string? BatchNumber { get; init; }
}

public class CreateInventoryTransactionCommandHandler : IRequestHandler<CreateInventoryTransactionCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateInventoryTransactionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateInventoryTransactionCommand request, CancellationToken cancellationToken)
    {
        // Validate warehouse exists
        var warehouse = await _unitOfWork.Repository<Warehouse>().GetByIdAsync(request.WarehouseId);
        if (warehouse == null)
            throw new Domain.Exceptions.BusinessException("المستودع المصدر غير موجود.");

        // For transfers, validate destination warehouse
        if (request.TransactionType == InventoryTransactionType.Transfer)
        {
            if (!request.ToWarehouseId.HasValue)
                throw new Domain.Exceptions.BusinessException("يجب تحديد المستودع الوجهة للتحويلات.");

            var toWarehouse = await _unitOfWork.Repository<Warehouse>().GetByIdAsync(request.ToWarehouseId.Value);
            if (toWarehouse == null)
                throw new Domain.Exceptions.BusinessException("المستودع الوجهة غير موجود.");

            if (request.WarehouseId == request.ToWarehouseId.Value)
                throw new Domain.Exceptions.BusinessException("لا يمكن التحويل إلى نفس المستودع.");
        }

        // Create the transaction master
        var transactionMaster = new InventoryTransactionMaster(
            Guid.NewGuid(),
            request.DocumentNumber,
            request.TransactionDate,
            request.TransactionType,
            request.WarehouseId,
            request.CreatedBy,
            request.Notes,
            request.ToWarehouseId
        );

        // Add lines
        foreach (var lineDto in request.Lines)
        {
            var item = await _unitOfWork.Repository<Item>().GetByIdAsync(lineDto.ItemId);
            if (item == null)
                throw new Domain.Exceptions.BusinessException($"الصنف بمعرف {lineDto.ItemId} غير موجود.");

            var unit = await _unitOfWork.Repository<Domain.Entities.Unit>().GetByIdAsync(lineDto.UnitId);
            if (unit == null)
                throw new Domain.Exceptions.BusinessException($"الوحدة بمعرف {lineDto.UnitId} غير موجودة.");

            var line = new InventoryTransactionLine(
                Guid.NewGuid(),
                transactionMaster.Id,
                lineDto.ItemId,
                lineDto.UnitId,
                lineDto.Quantity,
                lineDto.ConversionRate,
                lineDto.Price,
                lineDto.BatchNumber
            );

            transactionMaster.AddLine(line);
        }

        _unitOfWork.Repository<InventoryTransactionMaster>().Add(transactionMaster);
        await _unitOfWork.Complete();

        return transactionMaster.Id;
    }
}
