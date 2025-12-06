using MediatR;

namespace Order.Core.Application.UseCases.GetOrderByOrderNumber;

public record GetOrderByOrderNumberQuery(string OrderNumber) : IRequest<GetOrderByOrderNumberResponse>;
