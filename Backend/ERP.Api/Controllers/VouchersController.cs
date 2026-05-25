using ERP.Application.Features.Treasury.Vouchers.Commands.Create;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

public class VouchersController : ApiControllerBase
{
    /// <summary>
    /// إنشاء سند صرف جديد مع إنشاء القيد المحاسبي تلقائياً
    /// </summary>
    [HttpPost("payment")]
    public async Task<IActionResult> CreatePayment(CreatePaymentVoucherCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(CreatePayment), new { id = result }, result);
    }
}
