using ERP.Application.Features.Purchasing.Vendors;
using ERP.Application.Features.Purchasing.Vendors.Commands.Create;
using ERP.Application.Features.Purchasing.Vendors.Commands.Update;
using ERP.Application.Features.Purchasing.Vendors.Commands.Delete;
using ERP.Application.Features.Purchasing.Vendors.Queries.GetAllVendors;
using ERP.Application.Features.Purchasing.Vendors.Queries.GetVendorById;
using ERP.Application.Features.Purchasing.Vendors.Queries.GetVendorsWithPagination;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP.Api.Controllers;

public class VendorsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<VendorsPagedResponse>> GetAll([FromQuery] GetVendorsWithPaginationQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<VendorDto>>> GetAllList()
    {
        return Ok(await Mediator.Send(new GetAllVendorsQuery()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<VendorDto>> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetVendorByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateVendorCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateVendorCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteVendorCommand(id));
        return NoContent();
    }
}
