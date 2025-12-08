using Order.Core.Domain.Orders.Enums;
using Order.Core.Domain.Orders.Events;
using Order.Core.Domain.Orders.ValueObjects;
using OrderItem = Order.Core.Domain.Orders.OrderItem;
using Order.Core.Domain.Common;
using Order.Core.Domain.Orders.Rules; 

namespace Order.Core.Domain.Orders;

public class Order : IAggregateRoot
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
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    private Order() { }

    public static Order Create()
    {
        var now = DateTime.UtcNow;
        var orderNumber = OrderNumber.Create(now);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            Status = OrderStatus.Open,
            CreatedAt = now,
            Total = Money.FromDecimal(0)
        };
        order.AddDomainEvent(new OrderCreatedDomainEvent(order.Id, orderNumber.Value));
        
        return order;

    }

    public void AddItem(OrderItem item)
    {
        EnsureOrderIsOpen();
        Items.Add(item);
        Total = Total.Add(item.Subtotal);
        UpdatedAt = DateTime.UtcNow;
    }
    public void UpdateItemQuantity(OrderItem item, int delta)
    {
        EnsureOrderIsOpen();
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
        EnsureOrderIsOpen();
        Total = Total.Subtract(item.Subtotal);

        Items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        OrderStatusRule.EnsureOrderIsOpen(Status, OrderNumber.Value);
        OrderItemsRule.EnsureOrderHasItems(Items, OrderNumber.Value);

        Status = OrderStatus.Closed;
        var now = DateTime.UtcNow;
        ClosedAt = now;
        UpdatedAt = now;
    }

    private void EnsureOrderIsOpen()
    {
        OrderStatusRule.EnsureOrderIsOpen(Status, OrderNumber.Value);
    }
    public void AddDomainEvent(IDomainEvent domainEvent)
        => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

}
