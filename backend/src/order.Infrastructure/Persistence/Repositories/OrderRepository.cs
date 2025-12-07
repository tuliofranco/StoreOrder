using OrderEntity = Order.Core.Domain.Orders.Order;
using Order.Core.Domain.Orders.ValueObjects;
using Order.Core.Application.Abstractions.Repositories;
using Microsoft.EntityFrameworkCore;
using Order.Core.Domain.Orders.Enums;
using Order.Core.Application.Common;

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

    public async Task<int> CountAsync(CancellationToken ct = default)
    {
        return await _db.Orders
            .Where(o => o.DeletedAt == null)
            .CountAsync(ct);
    }
    public async Task<PagedResult<OrderEntity>> GetPagedAsync(
        int page,
        int pageSize,
        OrderStatus? status,
        CancellationToken ct = default)
    {

        if (page < 0) page = 0;
        if (pageSize <= 0) pageSize = 25;

        var query = _db.Orders
            .AsNoTracking()
            .Where(o => o.DeletedAt == null);

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        var totalItems = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip(page * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResult<OrderEntity>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }



}
