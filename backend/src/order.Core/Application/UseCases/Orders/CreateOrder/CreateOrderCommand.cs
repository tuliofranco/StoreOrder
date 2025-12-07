using MediatR;
using OrderEntity = Order.Core.Domain.Orders.Order;

namespace Order.Core.Application.UseCases.Orders.CreateOrder;

public record CreateOrderCommand() : IRequest<CreateOrderResponse>;
