using OrderEntity = Order.Core.Domain.Orders.Order;

namespace Order.Core.Application.Abstractions.Repositories;

public interface IOrderRepository
{
    Task AddAsync(OrderEntity order, CancellationToken ct = default);
    Task<OrderEntity?> GetByOrderNumberAsync(string OrderNumber, CancellationToken ct = default);
    Task<IReadOnlyList<OrderEntity?>> ListOrdersAsync(CancellationToken ct = default);

}