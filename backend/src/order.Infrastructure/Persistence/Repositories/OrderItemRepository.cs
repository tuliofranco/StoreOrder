 using OrderEntity = Order.Core.Domain.Orders.Order;
using Order.Core.Domain.Orders.ValueObjects;
using Order.Core.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;
using Order.Core.Domain.Orders;

namespace Order.Infrastructure.Persistence.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly StoreOrderDbContext _db;

    public OrderItemRepository(StoreOrderDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(OrderItem orderItem, CancellationToken ct = default)
    {
        await _db.OrderItems.AddAsync(orderItem, ct);
    }
}
