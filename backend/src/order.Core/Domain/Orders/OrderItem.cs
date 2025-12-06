using Order.Core.Domain.Orders.ValueObjects;

namespace Order.Core.Domain.Orders;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }

    public string ProductName { get; private set; } = null!;
    public Money UnitPrice { get; private set; }
    public int Quantity { get; private set; }

    private OrderItem() { }

    public OrderItem(string productName, Money unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required.", nameof(productName));
        
        Id = Guid.NewGuid();
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public Money GetTotal() => UnitPrice.Multiply(Quantity);
}
