using MediatR;
using Order.Core.Application.Abstractions.Repositories;
using Order.Core.Application.Common;
using Microsoft.Extensions.Logging;

namespace Order.Core.Application.UseCases.Orders.GetAllOrders;

public class GetAllOrdersQueryHandler
    : IRequestHandler<GetAllOrdersQuery, PagedResult<GetAllOrdersResponse>>
{
    private readonly IOrderRepository _repository;
    private readonly ILogger<GetAllOrdersQueryHandler> _logger;

    public GetAllOrdersQueryHandler(IOrderRepository repository, ILogger<GetAllOrdersQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedResult<GetAllOrdersResponse>> Handle(
        GetAllOrdersQuery request,
        CancellationToken ct)
    {
        var page = request.page < 0 ? 0 : request.page;
        var pageSize = request.pageSize < 0 ? 25 : request.pageSize;

        _logger.LogInformation(
            "Handling GetAllOrdersQuery. Page {Page}, PageSize {PageSize}, StatusFilter {Status}",
            page,
            pageSize,
            request.Status?.ToString() ?? "null");

        var totalItems = await _repository.CountAsync(ct);

        var pagedOrders = await _repository
            .GetPagedAsync(page, pageSize, request.Status, ct);

        var mappedItems = pagedOrders.Items.Select(order => new GetAllOrdersResponse(
            order.OrderNumber.Value,
            order.ClientName,
            order.Status.ToString(),
            order.CreatedAt,
            order.UpdatedAt,
            order.ClosedAt,
            order.Total.Amount
        )).ToList();

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        _logger.LogInformation(
            "GetAllOrdersQuery completed. Page {Page}, Returned {Count} items, TotalItems {TotalItems}, TotalPages {TotalPages}",
            page,
            mappedItems.Count,
            totalItems,
            totalPages);

        return new PagedResult<GetAllOrdersResponse>
        {
            Items = mappedItems,
            Page = pagedOrders.Page,
            PageSize = pagedOrders.PageSize,
            TotalItems = pagedOrders.TotalItems,
            TotalPages = pagedOrders.TotalPages
        };
    }
}
