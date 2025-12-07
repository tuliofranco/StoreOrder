namespace Order.Core.Application.UseCases.Orders.GetOrderByOrderNumber;

public record GetOrderByOrderNumberItemResponse(
    Guid Id,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal
);

public record GetOrderByOrderNumberResponse(
    string OrderNumber,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ClosedAt,
    decimal Total,
    IReadOnlyCollection<GetOrderByOrderNumberItemResponse> Items
);
