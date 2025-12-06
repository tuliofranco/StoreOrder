
using Order.Core.Domain.Orders.Enums;
using Order.Core.Domain.Orders.ValueObjects;

namespace Order.Core.Domain.Orders;

public class Order
{
    public Guid Id { get; private set; }
    public OrderNumber OrderNumber { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    public List<OrderItem> Items { get; private set; } = new();
    public Money Total { get; private set; }

    private Order() { }

    public static Order Create()
    {
        DateTime now = DateTime.UtcNow;
        OrderNumber orderNumber = OrderNumber.Create(now);

        return new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            Status = OrderStatus.Open,
            CreatedAt = now,
            Items = new List<OrderItem>(),
            Total = Money.FromDecimal(0)
        };
    }
}