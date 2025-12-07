using MediatR;
using Order.Core.Application.Common;

namespace Order.Core.Application.UseCases.Orders.GetAllOrders;

public record GetAllOrdersQuery(int page, int pageSize): IRequest<PagedResult<GetAllOrdersResponse>>;
