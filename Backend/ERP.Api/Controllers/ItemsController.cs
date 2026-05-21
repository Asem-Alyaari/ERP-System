using ERP.Application.Features.Inventory.Items;
using ERP.Application.Features.Inventory.Items.Commands.Create;
using ERP.Application.Features.Inventory.Items.Commands.Update;
using ERP.Application.Features.Inventory.Items.Commands.Delete;
using ERP.Application.Features.Inventory.Items.Queries.GetItemById;
using ERP.Application.Features.Inventory.Items.Queries.GetItemsWithPagination;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

public class ItemsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ItemsPagedResponse>> GetAll([FromQuery] GetItemsWithPaginationQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetItemByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateItemCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateItemCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteItemCommand(id));
        return NoContent();
    }
}
