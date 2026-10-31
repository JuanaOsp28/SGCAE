namespace SGCAE.Domain.Common;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
