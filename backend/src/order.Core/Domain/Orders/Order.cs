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
            Items = new List<OrderItem>(),
            Total = Money.FromDecimal(0)
        };
    }

    public void AddItem(string description, Money unitPrice, int quantity)
    {
        if (Status != OrderStatus.Open)
            throw new InvalidOperationException("Only open orders can receive items.");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

        var existing = Items.FirstOrDefault(i =>
            i.Description.Equals(description, StringComparison.OrdinalIgnoreCase) &&
            i.UnitPrice.Equals(unitPrice));

        if (existing is null)
        {
            var item = OrderItem.Create(
                orderId: Id,
                description: description,
                unitPrice: unitPrice,
                quantity: quantity
            );

            Items.Add(item);
        }
        else
        {
            existing.IncreaseQuantity(quantity);
        }

        RecalculateTotal();
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotal()
    {
        if (Items.Count == 0)
        {
            Total = Money.FromDecimal(0);
            return;
        }

        var totalAmount = Items
            .Select(i => i.GetTotal().Amount)
            .Sum();

        Total = Money.FromDecimal(totalAmount);
    }
}
