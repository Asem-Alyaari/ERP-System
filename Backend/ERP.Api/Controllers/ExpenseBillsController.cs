using ERP.Application.Features.Expenses.Bills.Commands.Create;
using ERP.Application.Features.Expenses.Bills.Commands.Post;
using ERP.Application.Features.Expenses.Bills.Queries.GetAll;
using ERP.Application.Features.Expenses.Bills.Queries.GetById;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[Route("api/[controller]")]
public class ExpenseBillsController : ApiControllerBase
{
    /// <summary>
    /// الحصول على جميع فواتير المصروفات
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllExpenseBills()
    {
        var result = await Mediator.Send(new GetAllExpenseBillsQuery());
        return Ok(result);
    }

    /// <summary>
    /// الحصول على فاتورة مصروفات بالمعرف
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetExpenseBillById(Guid id)
    {
        var result = await Mediator.Send(new GetExpenseBillByIdQuery(id));
        return Ok(result);
    }

    /// <summary>
    /// إنشاء فاتورة مصروفات جديدة
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateExpenseBill(CreateExpenseBillCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(CreateExpenseBill), new { id = result }, result);
    }

    /// <summary>
    /// ترحيل فاتورة المصروفات (إنشاء القيد المحاسبي تلقائياً)
    /// </summary>
    [HttpPost("{id}/post")]
    public async Task<IActionResult> PostExpenseBill(Guid id, [FromBody] PostExpenseBillRequest request)
    {
        var result = await Mediator.Send(new PostExpenseBillCommand(id, request.UserId));
        return Ok(result);
    }
}

public record PostExpenseBillRequest(string UserId);
