namespace ERP.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}
