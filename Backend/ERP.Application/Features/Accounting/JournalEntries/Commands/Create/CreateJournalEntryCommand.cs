using MediatR;

namespace ERP.Application.Features.Accounting.JournalEntries.Commands.Create;

public record CreateJournalEntryCommand : IRequest<Guid>
{
    public string VoucherNumber { get; init; } = string.Empty;
    public DateTime TransactionDate { get; init; }
    public string? Description { get; init; }
    public Guid FiscalPeriodId { get; init; }
    public string CreatedBy { get; init; } = string.Empty;

    public List<JournalEntryLineDto> Lines { get; init; } = new();
}

public record JournalEntryLineDto
{
    public Guid AccountId { get; init; }
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public Guid CurrencyId { get; init; }
    public decimal ExchangeRate { get; init; }
    public decimal? ForeignDebit { get; init; }
    public decimal? ForeignCredit { get; init; }
    public Guid? CostCenterId { get; init; }
    public string? Memo { get; init; }
}
