using MediatR;

namespace ERP.Application.Features.Accounting.OpeningBalances.Commands;

public record OpeningBalanceDto
{
    public Guid AccountId { get; init; }
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
    public Guid? CostCenterId { get; init; }
    public Guid? CurrencyId { get; init; }
}

public record SetOpeningBalancesCommand(
    Guid FiscalPeriodId,
    List<OpeningBalanceDto> Balances,
    string CreatedBy
) : IRequest<bool>;
