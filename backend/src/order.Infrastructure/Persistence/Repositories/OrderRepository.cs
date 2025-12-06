using OrderEntity = Order.Core.Domain.Orders.Order;
using Order.Core.Domain.Orders.ValueObjects;
using Order.Core.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Order.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly StoreOrderDbContext _db;

    public OrderRepository(StoreOrderDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(OrderEntity order, CancellationToken ct = default)
    {
        await _db.Orders.AddAsync(order, ct);
    }

    public async Task<OrderEntity?> GetByIdAsync(string OrderNumber, CancellationToken ct = default)

    {
        OrderNumber number = new OrderNumber(OrderNumber);
        OrderEntity? order = await _db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.OrderNumber == number, ct);

        return order;
    }
}
