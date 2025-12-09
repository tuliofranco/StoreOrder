using MediatR;
using Order.Core.Application.Abstractions;
using Order.Core.Application.Abstractions.Repositories;
using Order.Core.Application.Common.Exceptions;
using Order.Core.Application.Orders;
using Microsoft.Extensions.Logging;

namespace Order.Core.Application.UseCases.Orders.GetOrderByOrderNumber;

public class GetOrderByOrderNumberQueryHandler : IRequestHandler<GetOrderByOrderNumberQuery, GetOrderByOrderNumberResponse>
{
    private readonly IOrderRepository _repository;
    private readonly ICacheService _cache;
    private readonly ILogger<GetOrderByOrderNumberQueryHandler> _logger;

    public GetOrderByOrderNumberQueryHandler(IOrderRepository repository, ICacheService cache, ILogger<GetOrderByOrderNumberQueryHandler> logger)
    {
        _repository = repository;
        _cache = cache;
        _logger = logger;
    }


    public async Task<GetOrderByOrderNumberResponse> Handle(
        GetOrderByOrderNumberQuery request, 
        CancellationToken ct)
    {

        var cacheKey = OrderCacheKeys.ByOrderNumber(request.OrderNumber);

        _logger.LogInformation(
            "Handling GetOrderByOrderNumberQuery for OrderNumber {OrderNumber}. CacheKey {CacheKey}",
            request.OrderNumber,
            cacheKey);

        var cached = await _cache.GetAsync<GetOrderByOrderNumberResponse>(cacheKey, ct);
        if (cached is not null)
        {
            _logger.LogInformation(
                "Cache hit for OrderNumber {OrderNumber}",
                request.OrderNumber);

            return cached;
        }

        _logger.LogInformation(
            "Cache miss for OrderNumber {OrderNumber}. Loading from repository.",
            request.OrderNumber);

        var order = await _repository.GetByOrderNumberAsync(request.OrderNumber, ct);

        if (order is null)
        {
            _logger.LogWarning(
                "Order not found for OrderNumber {OrderNumber}",
                request.OrderNumber);

            throw new OrderNotFoundException("Order not found");
        }

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
            ClientName: order.ClientName,
            Status: order.Status.ToString(),
            CreatedAt: order.CreatedAt,
            UpdatedAt: order.UpdatedAt,
            ClosedAt: order.ClosedAt,
            Total: order.Total.Amount,
            Items: items
        );

        await _cache.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5), ct);

        _logger.LogInformation(
            "Order loaded and cached for OrderNumber {OrderNumber}",
            order.OrderNumber.Value);
            
        return response;
    }
}
