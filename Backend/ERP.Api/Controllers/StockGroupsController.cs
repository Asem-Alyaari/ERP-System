using ERP.Application.Features.Inventory.StockGroups;
using ERP.Application.Features.Inventory.StockGroups.Commands.Create;
using ERP.Application.Features.Inventory.StockGroups.Commands.Update;
using ERP.Application.Features.Inventory.StockGroups.Commands.Delete;
using ERP.Application.Features.Inventory.StockGroups.Queries.GetStockGroupById;
using ERP.Application.Features.Inventory.StockGroups.Queries.GetStockGroupsWithPagination;
using ERP.Application.Features.Inventory.StockGroups.Queries.GetStockGroups;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

public class StockGroupsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<StockGroupsPagedResponse>> GetAll([FromQuery] GetStockGroupsWithPaginationQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<StockGroupDto>>> GetList()
    {
        return Ok(await Mediator.Send(new GetStockGroupsQuery()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StockGroupDto>> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetStockGroupByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateStockGroupCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateStockGroupCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteStockGroupCommand(id));
        return NoContent();
    }
}
