using OrderEntity = Order.Core.Domain.Orders.Order;
using Order.Core.Application.Abstractions.Repositories;

namespace Order.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private static readonly List<OrderEntity> _orders = new();

    public Task AddAsync(OrderEntity order, CancellationToken ct = default)
    {
        _orders.Add(order);
        return Task.CompletedTask;
    }
}
