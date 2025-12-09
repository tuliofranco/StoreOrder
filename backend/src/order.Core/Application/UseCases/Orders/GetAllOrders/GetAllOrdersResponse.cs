namespace Order.Core.Application.UseCases.Orders.GetAllOrders;

public record GetAllOrdersResponse(
    string OrderNumber,
    string ClientName,
    string Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    DateTime? ClosedAt,
    decimal Total
);
