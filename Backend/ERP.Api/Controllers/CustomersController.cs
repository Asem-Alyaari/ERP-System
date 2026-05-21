using ERP.Application.Features.Sales.Customers;
using ERP.Application.Features.Sales.Customers.Commands.Create;
using ERP.Application.Features.Sales.Customers.Commands.Update;
using ERP.Application.Features.Sales.Customers.Commands.Delete;
using ERP.Application.Features.Sales.Customers.Queries.GetAllCustomers;
using ERP.Application.Features.Sales.Customers.Queries.GetCustomerById;
using ERP.Application.Features.Sales.Customers.Queries.GetCustomersWithPagination;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP.Api.Controllers;

public class CustomersController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<CustomersPagedResponse>> GetAll([FromQuery] GetCustomersWithPaginationQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("all")]
    public async Task<ActionResult<List<CustomerDto>>> GetAllList()
    {
        return Ok(await Mediator.Send(new GetAllCustomersQuery()));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetCustomerByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateCustomerCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateCustomerCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteCustomerCommand(id));
        return NoContent();
    }
}
