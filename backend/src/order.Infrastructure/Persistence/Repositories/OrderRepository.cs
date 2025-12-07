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

    public async Task<OrderEntity?> GetByOrderNumberAsync(string orderNumber, CancellationToken ct = default)
    {
        var number = new OrderNumber(orderNumber);

        return await _db.Orders
            .Include(o => o.Items)
            .Where(o => o.DeletedAt == null && o.OrderNumber == number)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<OrderEntity?>> ListOrdersAsync(CancellationToken ct = default)
    {

        return await _db.Orders
                    .AsNoTracking()
                    .Where(o => o.DeletedAt == null)
                    .OrderByDescending(o => o.CreatedAt)
                    .ToListAsync(ct);
    }
}
