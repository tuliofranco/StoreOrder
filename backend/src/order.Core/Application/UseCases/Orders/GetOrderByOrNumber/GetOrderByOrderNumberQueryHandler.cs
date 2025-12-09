using MediatR;
using Order.Core.Application.Abstractions;
using Order.Core.Application.Abstractions.Repositories;
using Order.Core.Application.Common.Exceptions;
using Order.Core.Application.Orders;

namespace Order.Core.Application.UseCases.Orders.GetOrderByOrderNumber;

public class GetOrderByOrderNumberQueryHandler : IRequestHandler<GetOrderByOrderNumberQuery, GetOrderByOrderNumberResponse>
{
    private readonly IOrderRepository _repository;
    private readonly ICacheService _cache;

    public GetOrderByOrderNumberQueryHandler(IOrderRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }


    public async Task<GetOrderByOrderNumberResponse> Handle(
        GetOrderByOrderNumberQuery request, 
        CancellationToken ct)
    {

        var cacheKey = OrderCacheKeys.ByOrderNumber(request.OrderNumber);

        var cached = await _cache.GetAsync<GetOrderByOrderNumberResponse>(cacheKey, ct);
        if (cached is not null)
            return cached;

        var order = await _repository.GetByOrderNumberAsync(request.OrderNumber, ct);

        if (order is null)
            throw new OrderNotFoundException("Order not found");

        var items = order.Items
            .Select(i => new GetOrderByOrderNumberItemResponse(
                ProductId: i.ProductId,
                Description: i.Description,
                Quantity: i.Quantity,
                UnitPrice: i.UnitPrice.Amount,
                Subtotal: i.Subtotal.Amount
            ))
            .ToList();


        var response = new GetOrderByOrderNumberResponse(
            OrderNumber: order.OrderNumber.Value,
            Status: order.Status.ToString(),
            CreatedAt: order.CreatedAt,
            UpdatedAt: order.UpdatedAt,
            ClosedAt: order.ClosedAt,
            Total: order.Total.Amount,
            Items: items
        );
        await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), ct);
        return response;
    }
}
