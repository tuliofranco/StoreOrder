using MediatR;

namespace Order.Core.Application.UseCases.Orders.GetAllOrders;

public record GetAllOrdersQuery: IRequest<IEnumerable<GetAllOrdersResponse>>;
