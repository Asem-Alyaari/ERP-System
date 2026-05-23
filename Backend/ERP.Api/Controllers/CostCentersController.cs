using ERP.Application.Features.Accounting.CostCenters;
using ERP.Application.Features.Accounting.CostCenters.Commands.Create;
using ERP.Application.Features.Accounting.CostCenters.Queries.GetCostCenters;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

/// <summary>
/// مراكز التكلفة — إنشاء وجلب مراكز التكلفة الشجرية.
/// </summary>
public class CostCentersController : ApiControllerBase
{
    /// <summary>
    /// جلب مراكز التكلفة.
    /// استخدم <c>onlyDetail=true</c> (الافتراضي) لاسترداد المراكز التفصيلية فقط
    /// الصالحة للربط بأسطر القيود المحاسبية.
    /// استخدم <c>onlyDetail=false</c> لاسترداد الشجرة الكاملة.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<CostCenterDto>>> GetAll([FromQuery] bool? onlyDetail = true)
    {
        var result = await Mediator.Send(new GetCostCentersQuery { OnlyDetail = onlyDetail });
        return Ok(result);
    }

    /// <summary>
    /// إنشاء مركز تكلفة جديد.
    /// يدعم إنشاء مراكز شجرية (يحتوي على مراكز أبناء) ومراكز تفصيلية (أوراق نهائية).
    /// يتحقق تلقائياً من تفرد الكود وصلاحية الأب.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateCostCenterCommand command)
    {
        var id = await Mediator.Send(command);
        return Ok(id);
    }
}
