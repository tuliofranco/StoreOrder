using MediatR;

namespace Order.Core.Application.UseCases.GetAllOrders;

public record GetAllOrdersQuery: IRequest<IEnumerable<GetAllOrdersResponse>>;
