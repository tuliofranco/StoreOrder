using Order.Core.Domain.Common;

namespace Order.Core.Domain.Orders.Events;

public sealed record OrderClosedDomainEvent(
    Guid OrderId,
    string OrderNumber,
    DateTime ClosedAt
) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}