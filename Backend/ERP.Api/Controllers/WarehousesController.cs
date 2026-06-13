using ERP.Application.Features.Inventory.Warehouses;
using ERP.Application.Features.Inventory.Warehouses.Commands.Create;
using ERP.Application.Features.Inventory.Warehouses.Commands.Update;
using ERP.Application.Features.Inventory.Warehouses.Commands.Delete;
using ERP.Application.Features.Inventory.Warehouses.Queries.GetWarehousesWithPagination;
using ERP.Application.Features.Inventory.Warehouses.Queries.GetWarehousesList;
using ERP.Application.Features.Inventory.Warehouses.Queries.GetWarehouseById;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

public class WarehousesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<WarehousesPagedResponse>> GetAll([FromQuery] GetWarehousesWithPaginationQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    /// <summary>
    /// قائمة المستودعات - مخصصة للقوائم المنسدلة
    /// </summary>
    [HttpGet("list")]
    public async Task<ActionResult<List<WarehouseDto>>> GetList()
    {
        var result = await Mediator.Send(new GetWarehousesListQuery());
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WarehouseDto>> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetWarehouseByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateWarehouseCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateWarehouseCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteWarehouseCommand(id));
        return NoContent();
    }
}
