using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Repositories;
using ERP.Domain.Specifications;
using MediatR;

namespace ERP.Application.Features.Accounting.JournalEntries.Queries.GetJournalEntryById;

public record GetJournalEntryByIdQuery(Guid Id) : IRequest<JournalEntryDetailDto?>;

public class JournalEntryDetailDto
{
    public Guid Id { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
    public string? Description { get; set; }
    public Guid FiscalPeriodId { get; set; }
    public string FiscalPeriodName { get; set; } = string.Empty;
    public JournalEntryStatus Status { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? PostedBy { get; set; }
    public DateTime? PostedAt { get; set; }
    public List<JournalEntryLineDetailDto> Lines { get; set; } = new();
}

public class JournalEntryLineDetailDto
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountNameAr { get; set; } = string.Empty;
    public string AccountNameEn { get; set; } = string.Empty;
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal? ForeignDebit { get; set; }
    public decimal? ForeignCredit { get; set; }
    public Guid CurrencyId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string CurrencyName { get; set; } = string.Empty;
    public decimal ExchangeRate { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }
    public string? CostCenterNameAr { get; set; }
    public string? CostCenterNameEn { get; set; }
    public string? Memo { get; set; }
}

public class GetJournalEntryByIdQueryHandler : IRequestHandler<GetJournalEntryByIdQuery, JournalEntryDetailDto?>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetJournalEntryByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<JournalEntryDetailDto?> Handle(GetJournalEntryByIdQuery request, CancellationToken cancellationToken)
    {
        var spec = new JournalEntryDetailSpecification(request.Id);
        var entry = (await _unitOfWork.Repository<JournalEntryMaster>().ListAsync(spec)).FirstOrDefault();
        if (entry == null) return null;

        return new JournalEntryDetailDto
        {
            Id = entry.Id,
            VoucherNumber = entry.VoucherNumber,
            TransactionDate = entry.TransactionDate,
            Description = entry.Description,
            FiscalPeriodId = entry.FiscalPeriodId,
            FiscalPeriodName = entry.FiscalPeriod?.YearName ?? string.Empty,
            Status = entry.Status,
            CreatedBy = entry.CreatedBy,
            CreatedAt = entry.CreatedAt,
            PostedBy = entry.PostedBy,
            PostedAt = entry.PostedAt,
            Lines = entry.Lines.Select(l => new JournalEntryLineDetailDto
            {
                Id = l.Id,
                AccountId = l.AccountId,
                AccountCode = l.Account?.AccountCode ?? string.Empty,
                AccountNameAr = l.Account?.AccountNameAr ?? string.Empty,
                AccountNameEn = l.Account?.AccountNameEn ?? string.Empty,
                Debit = l.Debit,
                Credit = l.Credit,
                ForeignDebit = l.ForeignDebit,
                ForeignCredit = l.ForeignCredit,
                CurrencyId = l.CurrencyId,
                CurrencyCode = l.Currency?.Code ?? string.Empty,
                CurrencyName = l.Currency?.Name ?? string.Empty,
                ExchangeRate = l.ExchangeRate,
                CostCenterId = l.CostCenterId,
                CostCenterCode = l.CostCenter?.CostCenterCode,
                CostCenterNameAr = l.CostCenter?.CostCenterNameAr,
                CostCenterNameEn = l.CostCenter?.CostCenterNameEn,
                Memo = l.Memo
            }).ToList()
        };
    }
}

public class JournalEntryDetailSpecification : BaseSpecification<JournalEntryMaster>
{
    public JournalEntryDetailSpecification(Guid id) : base(x => x.Id == id)
    {
        AddInclude(x => x.FiscalPeriod!);
        AddIncludeString("Lines.Account");
        AddIncludeString("Lines.Currency");
        AddIncludeString("Lines.CostCenter");
    }
}
