using MediatR;
using Order.Core.Application.Abstractions;
using Order.Core.Domain.Orders.Events;

namespace Order.Core.Application.Orders.EventHandlers;
public class OrderCacheInvalidationHandler :
    INotificationHandler<OrderCreatedDomainEvent>,
    INotificationHandler<OrderClosedDomainEvent>
{
    private readonly ICacheService _cache;

    public OrderCacheInvalidationHandler(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task Handle(OrderCreatedDomainEvent notification, CancellationToken ct)
        => await Invalidate(notification.OrderId, notification.OrderNumber, ct);

    public async Task Handle(OrderClosedDomainEvent notification, CancellationToken ct)
        => await Invalidate(notification.OrderId, notification.OrderNumber, ct);

    private async Task Invalidate(Guid orderId, string orderNumber, CancellationToken ct)
    {
        var keyByNumber = OrderCacheKeys.ByOrderNumber(orderNumber);
        var keyById = OrderCacheKeys.ById(orderId);

        await _cache.RemoveAsync(keyByNumber, ct);
        await _cache.RemoveAsync(keyById, ct);
    }
}
