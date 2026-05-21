using MediatR;

namespace ERP.Application.Features.Accounting.Reports.Queries.GetTrialBalance;

public record GetTrialBalanceQuery(Guid FiscalPeriodId) : IRequest<TrialBalanceResponse>;

public class TrialBalanceDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountNameAr { get; set; } = string.Empty;
    public string AccountNameEn { get; set; } = string.Empty;
    
    public decimal TotalDebit { get; set; }  // مجموع الحركات المدينة
    public decimal TotalCredit { get; set; } // مجموع الحركات الدائنة
    
    public decimal DebitBalance { get; set; }  // الرصيد المدين
    public decimal CreditBalance { get; set; } // الرصيد الدائن
}

public class TrialBalanceResponse
{
    public List<TrialBalanceDto> TrialBalanceLines { get; set; } = new();
    
    // إجماليات الميزان الكبرى
    public decimal SumTotalDebit { get; set; }
    public decimal SumTotalCredit { get; set; }
    public decimal SumDebitBalance { get; set; }
    public decimal SumCreditBalance { get; set; }
    
    public bool IsBalanced => SumTotalDebit == SumTotalCredit && SumDebitBalance == SumCreditBalance;
}
