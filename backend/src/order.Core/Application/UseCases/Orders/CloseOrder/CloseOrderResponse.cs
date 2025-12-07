namespace Order.Core.Application.UseCases.Orders.CloseOrder;

public sealed record CloseOrderResponse(
    string OrderNumber,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ClosedAt,
    decimal Total
);
