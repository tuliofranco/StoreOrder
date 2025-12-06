namespace Order.Core.Application.UseCases.GetAllOrders;

public record GetAllOrdersResponse(
    string OrderNumber,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ClosedAt,
    decimal Total
);
