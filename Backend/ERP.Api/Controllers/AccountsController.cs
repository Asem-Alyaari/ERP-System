using ERP.Application.Features.Accounting.Accounts.Commands.Create;
using ERP.Application.Features.Accounting.Accounts.Commands.Update;
using ERP.Application.Features.Accounting.Accounts.Commands.Delete;
using ERP.Application.Features.Accounting.Accounts.Queries.GetDetailAccounts;
using ERP.Application.Features.Accounting.Accounts.Queries.GetAccountsTree;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP.Api.Controllers;

public class AccountsController : ApiControllerBase
{
    /// <summary>
    /// الحصول على جميع الحسابات (شجرة الحسابات)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<AccountDto>>> GetAll()
    {
        return Ok(await Mediator.Send(new GetAccountsTreeQuery()));
    }

    /// <summary>
    /// إضافة حساب جديد لدليل الحسابات
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateAccountCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(Create), new { id = result }, result);
    }

    /// <summary>
    /// تعديل بيانات حساب في دليل الحسابات
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateAccountCommand command)
    {
        if (id != command.Id) return BadRequest("ID mismatch");
        await Mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// حذف حساب من دليل الحسابات
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await Mediator.Send(new DeleteAccountCommand(id));
        return NoContent();
    }

    /// <summary>
    /// الحصول على قائمة الحسابات التفصيلية (التحليلية)
    /// </summary>
    [HttpGet("detail")]
    public async Task<ActionResult<List<AccountLookupDto>>> GetDetailAccounts()
    {
        return Ok(await Mediator.Send(new GetDetailAccountsQuery()));
    }
}
