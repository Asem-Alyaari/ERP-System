using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERP.Domain.Entities;
using ERP.Domain.Repositories;
using ERP.Domain.Enums;
using ERP.Application.Features.Accounting.LedgerReport.Specifications;
using MediatR;

namespace ERP.Application.Features.Accounting.LedgerReport.Queries.GetLedgerReport;

public record GetLedgerReportQuery : IRequest<LedgerReportDto>
{
    public Guid AccountId { get; init; }
    public Guid? CostCenterId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
}

public class LedgerReportDto
{
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountNameAr { get; set; } = string.Empty;
    public string AccountNameEn { get; set; } = string.Empty;
    public Guid? CostCenterId { get; set; }
    public string? CostCenterName { get; set; }
    public string FromDate { get; set; } = string.Empty;
    public string ToDate { get; set; } = string.Empty;
    public LedgerSummaryDto Summary { get; set; } = new();
    public List<LedgerTransactionDto> Transactions { get; set; } = new();
}

public class LedgerSummaryDto
{
    public decimal OpeningBalance { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal ClosingBalance { get; set; }
}

public class LedgerTransactionDto
{
    public Guid Id { get; set; }
    public string Date { get; set; } = string.Empty;
    public string VoucherNumber { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
    public string? CostCenterName { get; set; }
}

public class GetLedgerReportQueryHandler : IRequestHandler<GetLedgerReportQuery, LedgerReportDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetLedgerReportQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<LedgerReportDto> Handle(GetLedgerReportQuery request, CancellationToken cancellationToken)
    {
        // Get the account
        var account = await _unitOfWork.Repository<Account>().GetByIdAsync(request.AccountId);
        if (account == null)
            throw new Exception("الحساب غير موجود");

        // Get cost center if specified
        CostCenter? costCenter = null;
        if (request.CostCenterId.HasValue)
        {
            costCenter = await _unitOfWork.Repository<CostCenter>().GetByIdAsync(request.CostCenterId.Value);
        }

        // Get all transactions in date range
        var spec = new LedgerTransactionSpecification(
            request.AccountId,
            request.CostCenterId,
            request.FromDate,
            request.ToDate,
            includeBeforeDate: false
        );

        var transactions = await _unitOfWork.Repository<JournalEntryLine>().ListAsync(spec);

        // Filter for posted entries only
        transactions = transactions
            .Where(l => l.JournalEntryMaster != null && l.JournalEntryMaster.Status == JournalEntryStatus.Posted)
            .ToList();

        // Calculate opening balance (sum of all transactions before the date range)
        var openingBalanceSpec = new LedgerTransactionSpecification(
            request.AccountId,
            request.CostCenterId,
            request.FromDate,
            request.ToDate,
            includeBeforeDate: true
        );

        var openingBalanceTransactions = await _unitOfWork.Repository<JournalEntryLine>().ListAsync(openingBalanceSpec);
        openingBalanceTransactions = openingBalanceTransactions
            .Where(l => l.JournalEntryMaster != null && l.JournalEntryMaster.Status == JournalEntryStatus.Posted)
            .ToList();

        var openingBalance = openingBalanceTransactions.Sum(l => l.Debit - l.Credit);

        // Calculate running balance
        var runningBalance = openingBalance;
        var transactionDtos = transactions.Select(t => new LedgerTransactionDto
        {
            Id = t.Id,
            Date = t.JournalEntryMaster?.TransactionDate.ToString("yyyy-MM-dd") ?? string.Empty,
            VoucherNumber = t.JournalEntryMaster?.VoucherNumber ?? string.Empty,
            Reference = t.JournalEntryMaster?.VoucherNumber,
            Description = t.JournalEntryMaster?.Description ?? string.Empty,
            Debit = t.Debit,
            Credit = t.Credit,
            RunningBalance = runningBalance += (t.Debit - t.Credit),
            CostCenterName = t.CostCenter?.CostCenterNameAr
        }).ToList();

        // Calculate totals
        var totalDebit = transactionDtos.Sum(t => t.Debit);
        var totalCredit = transactionDtos.Sum(t => t.Credit);
        var closingBalance = openingBalance + totalDebit - totalCredit;

        return new LedgerReportDto
        {
            AccountId = account.Id,
            AccountCode = account.AccountCode,
            AccountNameAr = account.AccountNameAr,
            AccountNameEn = account.AccountNameEn,
            CostCenterId = costCenter?.Id,
            CostCenterName = costCenter?.CostCenterNameAr,
            FromDate = request.FromDate?.ToString("yyyy-MM-dd") ?? DateTime.MinValue.ToString("yyyy-MM-dd"),
            ToDate = request.ToDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd"),
            Summary = new LedgerSummaryDto
            {
                OpeningBalance = openingBalance,
                TotalDebit = totalDebit,
                TotalCredit = totalCredit,
                ClosingBalance = closingBalance
            },
            Transactions = transactionDtos
        };
    }
}
