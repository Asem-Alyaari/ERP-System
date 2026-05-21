using ERP.Application.Features.Accounting.Accounts.Commands.Create;
using ERP.Application.Features.Accounting.Accounts.Queries.GetDetailAccounts;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERP.Api.Controllers;

public class AccountsController : ApiControllerBase
{
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
    /// الحصول على قائمة الحسابات التفصيلية (التحليلية)
    /// </summary>
    [HttpGet("detail")]
    public async Task<ActionResult<List<AccountLookupDto>>> GetDetailAccounts()
    {
        return Ok(await Mediator.Send(new GetDetailAccountsQuery()));
    }
}
