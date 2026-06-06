using ERP.Application.Features.Purchasing.Invoices.Commands.Post;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

[Route("api/[controller]")]
public class PurchaseInvoicesController : ApiControllerBase
{
    /// <summary>
    /// ترحيل فاتورة المشتريات (إنشاء القيد المحاسبي تلقائياً)
    /// </summary>
    [HttpPost("{id}/post")]
    public async Task<IActionResult> PostPurchaseInvoice(Guid id, [FromBody] PostPurchaseInvoiceRequest request)
    {
        var result = await Mediator.Send(new PostPurchaseInvoiceCommand(id, request.UserId));
        return Ok(result);
    }
}

public record PostPurchaseInvoiceRequest(string UserId);
