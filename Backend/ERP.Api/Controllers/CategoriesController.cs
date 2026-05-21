using ERP.Application.Features.Inventory.Categories;
using ERP.Application.Features.Inventory.Categories.Commands.Create;
using ERP.Application.Features.Inventory.Categories.Commands.Update;
using ERP.Application.Features.Inventory.Categories.Commands.Delete;
using ERP.Application.Features.Inventory.Categories.Queries.GetAllCategories;
using ERP.Application.Features.Inventory.Categories.Queries.GetCategoryById;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

public class CategoriesController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ERP.Application.Features.Inventory.Categories.Queries.GetCategoriesWithPagination.CategoriesPagedResponse>> GetAll([FromQuery] ERP.Application.Features.Inventory.Categories.Queries.GetCategoriesWithPagination.GetCategoriesWithPaginationQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetCategoryByIdQuery(id));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateCategoryCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(Guid id, UpdateCategoryCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        await Mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteCategoryCommand(id));
        return NoContent();
    }
}
