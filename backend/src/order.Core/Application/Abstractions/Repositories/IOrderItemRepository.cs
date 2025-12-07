using Order.Core.Domain.Orders.ValueObjects;
using OrderItem = Order.Core.Domain.Orders.OrderItem;

namespace Order.Core.Application.Abstractions.Repositories;

public interface IOrderItemRepository
{
    Task AddAsync(OrderItem orderItem, CancellationToken ct = default);
}