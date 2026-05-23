using ERP.Application.Features.Inventory.StockGroups;
using ERP.Application.Features.Inventory.StockGroups.Commands.Create;
using ERP.Application.Features.Inventory.StockGroups.Commands.Delete;
using ERP.Application.Features.Inventory.StockGroups.Commands.Update;
using ERP.Application.Features.Inventory.StockGroups.Queries.GetStockGroupById;
using ERP.Application.Features.Inventory.StockGroups.Queries.GetStockGroups;
using ERP.Application.Features.Inventory.StockGroups.Queries.GetStockGroupsDropdown;
using ERP.Application.Features.Inventory.StockGroups.Queries.GetStockGroupsTree;
using ERP.Application.Features.Inventory.StockGroups.Queries.GetStockGroupsWithPagination;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

public class StockGroupsController : ApiControllerBase
{
    /// <summary>
    /// جلب مجموعات الأصناف مع دعم التصفية.
    /// استخدم <c>onlyDetails=true</c> لجلب المجموعات التفصيلية فقط (لقوائم الأصناف المنسدلة).
    /// </summary>
    [HttpGet("list")]
    public async Task<ActionResult<List<StockGroupDto>>> GetList([FromQuery] bool? onlyDetails)
    {
        var result = await Mediator.Send(new GetStockGroupsQuery { OnlyDetails = onlyDetails });
        return Ok(result);
    }

    /// <summary>
    /// نقطة نهاية مخصصة لقوائم الاختيار المنسدلة في شاشة إنشاء/تعديل الأصناف.
    /// يُعيد فقط المجموعات التفصيلية (IsDetail == true) مع الحسابات المحاسبية الموروثة.
    /// </summary>
    [HttpGet("dropdown")]
    public async Task<ActionResult<List<StockGroupDropdownDto>>> GetDropdown()
    {
        var result = await Mediator.Send(new GetStockGroupsDropdownQuery());
        return Ok(result);
    }

    /// <summary>
    /// جلب الهيكل الشجري الكامل لمجموعات الأصناف.
    /// يُستخدم في شاشة تهيئة النظام وعرض الشجرة.
    /// </summary>
    [HttpGet("tree")]
    public async Task<ActionResult<List<StockGroupTreeDto>>> GetTree()
    {
        var result = await Mediator.Send(new GetStockGroupsTreeQuery());
        return Ok(result);
    }

    /// <summary>
    /// جلب مجموعات الأصناف مع Pagination للجداول الإدارية.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<StockGroupsPagedResponse>> GetAll([FromQuery] GetStockGroupsWithPaginationQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    /// <summary>
    /// جلب مجموعة أصناف بالمعرّف.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StockGroupDto>> GetById(Guid id)
    {
        var result = await Mediator.Send(new GetStockGroupByIdQuery(id));
        if (result is null) return NotFound();
        return Ok(result);
    }

    /// <summary>
    /// إنشاء مجموعة أصناف جديدة.
    /// إذا تُركت الحسابات المالية فارغة، يتولّى النظام توليدها أو وراثتها تلقائياً.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateStockGroupCommand command)
    {
        var id = await Mediator.Send(command);
        return Ok(id);
    }

    /// <summary>
    /// تحديث مجموعة أصناف موجودة.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, UpdateStockGroupCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// حذف مجموعة أصناف.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteStockGroupCommand(id));
        return NoContent();
    }
}
