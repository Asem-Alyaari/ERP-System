using System;
using ERP.Domain.Entities;
using ERP.Domain.Enums;
using ERP.Domain.Specifications;

namespace ERP.Application.Features.Accounting.JournalEntries.Specifications;

public class JournalEntryFilterSpecification : BaseSpecification<JournalEntryMaster>
{
    public JournalEntryFilterSpecification(
        string? searchTerm, 
        JournalEntryStatus? status, 
        Guid? fiscalPeriodId, 
        DateTime? startDate, 
        DateTime? endDate, 
        int skip, 
        int take) 
        : base(x => 
            (string.IsNullOrEmpty(searchTerm) || x.VoucherNumber.Contains(searchTerm) || (x.Description != null && x.Description.Contains(searchTerm))) &&
            (!status.HasValue || x.Status == status.Value) &&
            (!fiscalPeriodId.HasValue || x.FiscalPeriodId == fiscalPeriodId.Value) &&
            (!startDate.HasValue || x.TransactionDate >= startDate.Value) &&
            (!endDate.HasValue || x.TransactionDate <= endDate.Value)
        )
    {
        AddInclude(x => x.FiscalPeriod!);
        AddInclude(x => x.Lines);
        ApplyOrderByDescending(x => x.TransactionDate);
        ApplyPaging(skip, take);
    }
}
