using MediatR;
using OrderEntity = Order.Core.Domain.Orders.Order;

namespace Order.Core.Application.UseCases.CreateOrder;

public record CreateOrderCommand() : IRequest<CreateOrderResponse>;
