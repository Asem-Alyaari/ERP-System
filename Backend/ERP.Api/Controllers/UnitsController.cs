using ERP.Application.Features.Inventory.Units;
using ERP.Application.Features.Inventory.Units.Commands.Create;
using ERP.Application.Features.Inventory.Units.Commands.Update;
using ERP.Application.Features.Inventory.Units.Commands.Delete;
using ERP.Application.Features.Inventory.Units.Queries.GetAllUnits;
using ERP.Application.Features.Inventory.Units.Queries.GetUnitById;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

public class UnitsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ERP.Application.Features.Inventory.Units.Queries.GetUnitsWithPagination.UnitsPagedResponse>> GetAll([FromQuery] ERP.Application.Features.Inventory.Units.Queries.GetUnitsWithPagination.GetUnitsWithPaginationQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UnitDto>> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetUnitByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateUnitCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateUnitCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteUnitCommand(id));
        return NoContent();
    }
}
