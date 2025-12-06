namespace Order.Core.Application.UseCases.CreateOrder;

public record CreateOrderResponse(
    string OrderNumber,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ClosedAt,
    decimal Total
);
