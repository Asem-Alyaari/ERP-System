using ERP.Application.Features.Accounting.JournalEntries.Commands.Create;
using ERP.Application.Features.Accounting.JournalEntries.Commands.Post;
using ERP.Application.Features.Accounting.JournalEntries.Commands.Unpost;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

public class JournalEntriesController : ApiControllerBase
{
    /// <summary>
    /// إنشاء قيد يومية جديد (مسودة)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateJournalEntryCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id = result }, result);
    }

    /// <summary>
    /// ترحيل قيد يومية وتحديث الأرصدة التراكمية
    /// </summary>
    [HttpPost("{id}/post")]
    public async Task<IActionResult> Post(Guid id, [FromBody] string postedBy)
    {
        var result = await Mediator.Send(new PostJournalEntryCommand(id, postedBy));
        return Ok(result);
    }

    /// <summary>
    /// إلغاء ترحيل قيد يومية وعكس الأرصدة
    /// </summary>
    [HttpPost("{id}/unpost")]
    public async Task<IActionResult> Unpost(Guid id, [FromBody] string unpostedBy)
    {
        var result = await Mediator.Send(new UnpostJournalEntryCommand(id, unpostedBy));
        return Ok(result);
    }
}
