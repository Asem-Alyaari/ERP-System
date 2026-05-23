using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Repositories;
using ERP.Application.Features.Accounting.JournalEntries.Specifications;
using MediatR;

namespace ERP.Application.Features.Accounting.JournalEntries.Queries.GetJournalEntriesWithPagination;

public record GetJournalEntriesWithPaginationQuery : IRequest<JournalEntriesPagedResponse>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public JournalEntryStatus? Status { get; init; }
    public Guid? FiscalPeriodId { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
}

public class JournalEntriesPagedResponse
{
    public List<JournalEntryDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class JournalEntryDto
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
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
}

public class GetJournalEntriesWithPaginationQueryHandler : IRequestHandler<GetJournalEntriesWithPaginationQuery, JournalEntriesPagedResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetJournalEntriesWithPaginationQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<JournalEntriesPagedResponse> Handle(GetJournalEntriesWithPaginationQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.PageNumber - 1) * request.PageSize;

        var spec = new JournalEntryFilterSpecification(
            request.SearchTerm, 
            request.Status, 
            request.FiscalPeriodId, 
            request.StartDate, 
            request.EndDate, 
            skip, 
            request.PageSize
        );

        var countSpec = new JournalEntryFilterCountSpecification(
            request.SearchTerm, 
            request.Status, 
            request.FiscalPeriodId, 
            request.StartDate, 
            request.EndDate
        );

        var items = await _unitOfWork.Repository<JournalEntryMaster>().ListAsync(spec);
        var totalCount = await _unitOfWork.Repository<JournalEntryMaster>().CountAsync(countSpec);

        return new JournalEntriesPagedResponse
        {
            Items = items.Select(x => new JournalEntryDto
            {
                Id = x.Id,
                VoucherNumber = x.VoucherNumber,
                TransactionDate = x.TransactionDate,
                Description = x.Description,
                FiscalPeriodId = x.FiscalPeriodId,
                FiscalPeriodName = x.FiscalPeriod?.YearName ?? string.Empty,
                Status = x.Status,
                CreatedBy = x.CreatedBy,
                CreatedAt = x.CreatedAt,
                PostedBy = x.PostedBy,
                PostedAt = x.PostedAt,
                TotalDebit = x.Lines.Sum(l => l.Debit),
                TotalCredit = x.Lines.Sum(l => l.Credit)
            }).ToList(),
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}
