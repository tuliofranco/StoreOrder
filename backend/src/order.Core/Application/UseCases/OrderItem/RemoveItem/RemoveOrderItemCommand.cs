using MediatR;

namespace Order.Core.Application.UseCases.OrderItem.RemoveItem;

public record RemoveOrderItemCommand(
    string OrderNumber,
    string ProductId
) : IRequest<RemoveOrderItemResponse>;
