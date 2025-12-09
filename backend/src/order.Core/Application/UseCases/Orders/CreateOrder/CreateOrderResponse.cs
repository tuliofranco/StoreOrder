namespace Order.Core.Application.UseCases.Orders.CreateOrder;

public record CreateOrderResponse(
    string OrderNumber,
    string clientName,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ClosedAt,
    decimal Total
);
