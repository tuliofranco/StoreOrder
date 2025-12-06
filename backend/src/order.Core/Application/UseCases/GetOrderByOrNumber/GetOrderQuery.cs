using MediatR;

namespace Order.Core.Application.UseCases.GetOrderByOrNumber;

public record GetOrderQuery(string OrderNumber) : IRequest<GetOrderResponse>;
