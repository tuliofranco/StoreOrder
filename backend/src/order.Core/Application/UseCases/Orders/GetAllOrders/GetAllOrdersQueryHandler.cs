using MediatR;
using Order.Core.Application.Abstractions.Repositories;
using Order.Core.Application.Common;

namespace Order.Core.Application.UseCases.Orders.GetAllOrders;

public class GetAllOrdersQueryHandler 
    : IRequestHandler<GetAllOrdersQuery, PagedResult<GetAllOrdersResponse>>
{
    private readonly IOrderRepository _repository;

    public GetAllOrdersQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<GetAllOrdersResponse>> Handle(
        GetAllOrdersQuery request,
        CancellationToken ct)
    {
        var page = request.page < 0 ? 0 : request.page;
        var pageSize = request.pageSize < 0 ? 25 : request.pageSize;

        var totalItems = await _repository.CountAsync(ct);

        var orders = await _repository
            .GetPagedAsync(page, pageSize, ct);

        var items = orders.Select(order => new GetAllOrdersResponse(
            order.OrderNumber.Value,
            order.Status.ToString(),
            order.CreatedAt,
            order.UpdatedAt,
            order.ClosedAt,
            order.Total.Amount
        )).ToList();
        
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResult<GetAllOrdersResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };
    }
}
