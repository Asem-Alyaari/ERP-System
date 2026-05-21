using ERP.Application.Features.Accounting.Currencies;
using ERP.Application.Features.Accounting.Currencies.Commands.Create;
using ERP.Application.Features.Accounting.Currencies.Commands.Update;
using ERP.Application.Features.Accounting.Currencies.Commands.Delete;
using ERP.Application.Features.Accounting.Currencies.Queries.GetCurrencyById;
using ERP.Application.Features.Accounting.Currencies.Queries.GetCurrenciesWithPagination;
using ERP.Application.Features.Accounting.Currencies.Queries.GetCurrencies;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

public class CurrenciesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CurrenciesPagedResponse>> GetAll([FromQuery] GetCurrenciesWithPaginationQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<CurrencyDto>>> GetList()
    {
        return Ok(await Mediator.Send(new GetCurrenciesQuery()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CurrencyDto>> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetCurrencyByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateCurrencyCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateCurrencyCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteCurrencyCommand(id));
        return NoContent();
    }
}
