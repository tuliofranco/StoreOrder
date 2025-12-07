using Order.Core.Domain.Orders.Enums;
using Order.Core.Domain.Orders.ValueObjects;
using OrderItem = Order.Core.Domain.Orders.OrderItem;

namespace Order.Core.Domain.Orders;

public class Order
{
    public Guid Id { get; private set; }
    public OrderNumber OrderNumber { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public List<OrderItem> Items { get; private set; } = new();
    public Money Total { get; private set; }

    private Order() { }

    public static Order Create()
    {
        var now = DateTime.UtcNow;
        var orderNumber = OrderNumber.Create(now);

        return new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            Status = OrderStatus.Open,
            CreatedAt = now,
            Total = Money.FromDecimal(0)
        };
    }

    public void AddItem(OrderItem item)
    {
        Items.Add(item);
        Total = Total.Add(item.Subtotal);
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateItemQuantity(OrderItem item, int delta)
    {
        var oldSubtotal = item.Subtotal;

        item.ChangeQuantity(delta);

        if (item.Quantity == 0)
        {
            Items.Remove(item);
            Total = Total.Subtract(oldSubtotal);
        }
        else
        {
            var newSubtotal = item.Subtotal;

            Total = Total
                .Subtract(oldSubtotal)
                .Add(newSubtotal);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveItem(OrderItem item)
    {
        Total = Total.Subtract(item.Subtotal);

        Items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }
}
