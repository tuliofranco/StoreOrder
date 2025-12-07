using MediatR;
using Order.Core.Application.Common;
using Order.Core.Domain.Orders.Enums;

namespace Order.Core.Application.UseCases.Orders.GetAllOrders;

public record GetAllOrdersQuery(int page, int pageSize, OrderStatus? Status): IRequest<PagedResult<GetAllOrdersResponse>>;
