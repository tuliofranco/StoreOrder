using MediatR;

namespace Order.Core.Application.UseCases.Orders.GetOrderByOrderNumber;

public record GetOrderByOrderNumberQuery(string OrderNumber) : IRequest<GetOrderByOrderNumberResponse>;
