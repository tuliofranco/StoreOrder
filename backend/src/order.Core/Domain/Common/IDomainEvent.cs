using MediatR;

namespace Order.Core.Domain.Common;

public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
}
