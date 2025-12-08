using Order.Core.Domain.Common;

namespace Order.Core.Domain.Orders.Events;

public record OrderCreatedDomainEvent(Guid OrderId, string OrderNumber) : IDomainEvent
{
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}
