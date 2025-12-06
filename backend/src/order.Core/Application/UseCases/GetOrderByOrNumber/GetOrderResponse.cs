namespace Order.Core.Application.UseCases.GetOrderByOrNumber;

public record GetOrderResponse(
    string OrderNumber,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ClosedAt,
    decimal Total
);
