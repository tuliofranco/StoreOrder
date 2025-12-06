namespace Order.Core.Application.UseCases.GetOrderByOrderNumber;

public record GetOrderByOrderNumberResponse(
    string OrderNumber,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ClosedAt,
    decimal Total
);
