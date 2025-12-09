namespace Order.Core.Application.UseCases.OrderItem.AddItem;

public class AddOrderItemResponse
{
    public string OrderNumber { get; init; } = string.Empty;
    public string ClientName { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public decimal Total { get; init; }

    public IReadOnlyCollection<AddOrderItemResponseItem> Items { get; init; } 
        = Array.Empty<AddOrderItemResponseItem>();
}

public class AddOrderItemResponseItem
{
    public string? ProductId { get; init; }
    public string Description { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public decimal Subtotal { get; init; }
}
