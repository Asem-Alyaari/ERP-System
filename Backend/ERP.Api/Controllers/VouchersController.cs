using ERP.Application.Features.Treasury.Vouchers.Commands.Create;
using ERP.Application.Features.Treasury.Vouchers.Commands.Post;
using ERP.Application.Features.Treasury.Vouchers.Queries.GetAllPaymentVouchers;
using ERP.Application.Features.Treasury.Vouchers.Queries.GetAllReceiptVouchers;
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

    /// <summary>
    /// إنشاء سند قبض جديد
    /// </summary>
    [HttpPost("receipt")]
    public async Task<IActionResult> CreateReceipt(CreateReceiptVoucherCommand command)
    {
        var result = await Mediator.Send(command);
        return CreatedAtAction(nameof(CreateReceipt), new { id = result }, result);
    }

    /// <summary>
    /// الحصول على جميع سندات القبض
    /// </summary>
    [HttpGet("receipt")]
    public async Task<IActionResult> GetAllReceiptVouchers()
    {
        var result = await Mediator.Send(new GetAllReceiptVouchersQuery());
        return Ok(result);
    }

    /// <summary>
    /// ترحيل سند قبض
    /// </summary>
    [HttpPost("receipt/{id}/post")]
    public async Task<IActionResult> PostReceiptVoucher(Guid id, [FromBody] PostPaymentVoucherRequest request)
    {
        var result = await Mediator.Send(new PostReceiptVoucherCommand(id, request.UserId));
        return Ok(result);
    }
}

public record PostPaymentVoucherRequest(string UserId);
