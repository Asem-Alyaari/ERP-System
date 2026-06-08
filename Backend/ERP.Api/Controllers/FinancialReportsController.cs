using ERP.Application.Features.Accounting.Reports.Queries.GetTrialBalance;
using ERP.Application.Features.Accounting.LedgerReport.Queries.GetLedgerReport;
using ERP.Application.Features.Accounting.FinancialReports.Queries.GetIncomeStatement;
using ERP.Application.Features.Accounting.FinancialReports.Queries.GetBalanceSheet;
using Microsoft.AspNetCore.Mvc;

namespace ERP.Api.Controllers;

public class FinancialReportsController : ApiControllerBase
{
    /// <summary>
    /// جلب ميزان المراجعة بناءً على الفترة المالية والفلاتر الاختيارية
    /// </summary>
    [HttpGet("trial-balance")]
    public async Task<ActionResult<TrialBalanceResponse>> GetTrialBalance([FromQuery] GetTrialBalanceQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// جلب تقرير كشف الحساب ودفتر الأستاذ العام
    /// </summary>
    [HttpGet("ledger")]
    public async Task<ActionResult<LedgerReportDto>> GetLedgerReport([FromQuery] GetLedgerReportQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// جلب قائمة الدخل (Income Statement)
    /// </summary>
    [HttpGet("income-statement")]
    public async Task<ActionResult<IncomeStatementDto>> GetIncomeStatement([FromQuery] GetIncomeStatementQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// جلب الميزانية العمودية (Balance Sheet)
    /// </summary>
    [HttpGet("balance-sheet")]
    public async Task<ActionResult<BalanceSheetDto>> GetBalanceSheet([FromQuery] GetBalanceSheetQuery query)
    {
        var result = await Mediator.Send(query);
        return Ok(result);
    }
}
