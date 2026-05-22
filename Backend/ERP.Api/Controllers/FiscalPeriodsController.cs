using ERP.Application.Features.Accounting.FiscalPeriods;
using ERP.Application.Features.Accounting.FiscalPeriods.Commands.Create;
using ERP.Application.Features.Accounting.FiscalPeriods.Commands.Close;
using ERP.Application.Features.Accounting.FiscalPeriods.Commands.Open;
using ERP.Application.Features.Accounting.FiscalPeriods.Commands.Delete;
using ERP.Application.Features.Accounting.FiscalPeriods.Queries.GetFiscalPeriods;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

public class FiscalPeriodsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<FiscalPeriodDto>>> GetAll()
    {
        return Ok(await Mediator.Send(new GetFiscalPeriodsQuery()));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateFiscalPeriodCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPut("{id}/close")]
    public async Task<ActionResult> Close(Guid id)
    {
        await Mediator.Send(new CloseFiscalPeriodCommand(id));
        return NoContent();
    }

    [HttpPut("{id}/open")]
    public async Task<ActionResult> Open(Guid id)
    {
        await Mediator.Send(new OpenFiscalPeriodCommand(id));
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteFiscalPeriodCommand(id));
        return NoContent();
    }
}
