using MediatR;

namespace Order.Core.Application.UseCases.OrderItem.AddItem;

public record AddOrderItemCommand(
    string OrderNumber,
    string Description,
    decimal UnitPrice,
    int Quantity
) : IRequest<AddOrderItemResponse>;
