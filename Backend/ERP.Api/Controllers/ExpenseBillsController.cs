using ERP.Application.Features.Expenses.Bills.Commands.Create;
using ERP.Application.Features.Expenses.Bills.Commands.Post;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

public class ExpenseBillsController : ApiControllerBase
{
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
