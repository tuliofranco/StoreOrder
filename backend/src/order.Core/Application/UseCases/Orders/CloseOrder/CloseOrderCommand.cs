using MediatR;

namespace Order.Core.Application.UseCases.Orders.CloseOrder;

public sealed record CloseOrderCommand(string OrderNumber) : IRequest<CloseOrderResponse>;
