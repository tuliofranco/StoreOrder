using Order.Core.Domain.Orders.ValueObjects;

namespace Order.Core.Domain.Orders;

public class OrderItem
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public string Description { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money Subtotal => UnitPrice.Multiply(Quantity);

    private OrderItem() { }

    private OrderItem(Guid orderId, string description, Money unitPrice, int quantity)
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("Order id is required.", nameof(orderId));

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");

        Id = Guid.NewGuid();
        OrderId = orderId;
        Description = description.Trim();
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    internal static OrderItem Create(
        Guid orderId,
        string description,
        Money unitPrice,
        int quantity
    ) => new(orderId, description, unitPrice, quantity);

    public void ChangeQuantity(int delta)
    {
        if (delta == 0)
            return;

        int newQuantity = Quantity + delta;

        if (newQuantity < 0)
            throw new InvalidOperationException("Quantity cannot be zero or negative.");

        Quantity = newQuantity;
    }

    public Money GetTotal() => Subtotal;
}