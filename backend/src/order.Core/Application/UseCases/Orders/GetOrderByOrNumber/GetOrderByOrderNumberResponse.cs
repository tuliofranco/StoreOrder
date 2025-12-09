namespace Order.Core.Application.UseCases.Orders.GetOrderByOrderNumber;

public record GetOrderByOrderNumberItemResponse(
    string ProductId,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal
);

public record GetOrderByOrderNumberResponse(
    string OrderNumber,
    string ClientName,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ClosedAt,
    decimal Total,
    IReadOnlyCollection<GetOrderByOrderNumberItemResponse> Items
);
