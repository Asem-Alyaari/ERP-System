using ERP.Application.Features.Accounting.CurrencyExchangeRates;
using ERP.Application.Features.Accounting.CurrencyExchangeRates.Commands.Create;
using ERP.Application.Features.Accounting.CurrencyExchangeRates.Commands.Update;
using ERP.Application.Features.Accounting.CurrencyExchangeRates.Commands.Delete;
using ERP.Application.Features.Accounting.CurrencyExchangeRates.Queries.GetCurrencyExchangeRateById;
using ERP.Application.Features.Accounting.CurrencyExchangeRates.Queries.GetCurrencyExchangeRatesWithPagination;
using ERP.Application.Features.Accounting.CurrencyExchangeRates.Queries.GetCurrencyExchangeRates;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

public class CurrencyExchangeRatesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CurrencyExchangeRatesPagedResponse>> GetAll([FromQuery] GetCurrencyExchangeRatesWithPaginationQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("list")]
    public async Task<ActionResult<List<CurrencyExchangeRateDto>>> GetList([FromQuery] Guid? currencyId)
    {
        return Ok(await Mediator.Send(new GetCurrencyExchangeRatesQuery(currencyId)));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CurrencyExchangeRateDto>> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetCurrencyExchangeRateByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateCurrencyExchangeRateCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateCurrencyExchangeRateCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteCurrencyExchangeRateCommand(id));
        return NoContent();
    }
}
