using ERP.Application.Features.Treasury.Vouchers.Commands.Create;
using ERP.Application.Features.Treasury.Vouchers.Commands.Post;
using ERP.Application.Features.Treasury.Vouchers.Queries.GetAllPaymentVouchers;
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

    /// <summary>
    /// الحصول على جميع سندات الصرف
    /// </summary>
    [HttpGet("payment")]
    public async Task<IActionResult> GetAllPaymentVouchers()
    {
        var result = await Mediator.Send(new GetAllPaymentVouchersQuery());
        return Ok(result);
    }

    /// <summary>
    /// ترحيل سند صرف
    /// </summary>
    [HttpPost("payment/{id}/post")]
    public async Task<IActionResult> PostPaymentVoucher(Guid id, [FromBody] PostPaymentVoucherRequest request)
    {
        var result = await Mediator.Send(new PostPaymentVoucherCommand(id, request.UserId));
        return Ok(result);
    }
}

public record PostPaymentVoucherRequest(string UserId);
