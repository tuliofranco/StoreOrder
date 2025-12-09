using System.ComponentModel.DataAnnotations;
using MediatR;
using Order.Core.Application.Abstractions;
using Order.Core.Application.Abstractions.Repositories;
using Order.Core.Application.Common.Exceptions;
using Order.Core.Domain.Orders.Enums;
using Order.Core.Domain.Orders.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Order.Core.Application.UseCases.OrderItem.RemoveItem;

public class RemoveOrderItemCommandHandler
    : IRequestHandler<RemoveOrderItemCommand, RemoveOrderItemResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<RemoveOrderItemCommandHandler> _logger;

    public RemoveOrderItemCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork uow,
        ILogger<RemoveOrderItemCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _uow = uow;
        _logger = logger;
    }

    public async Task<RemoveOrderItemResponse> Handle(
        RemoveOrderItemCommand request,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Handling RemoveOrderItemCommand for OrderNumber {OrderNumber}, ProductId {ProductId}",
            request.OrderNumber,
            request.ProductId);

        var order = await _orderRepository.GetByOrderNumberAsync(request.OrderNumber);

        if (order is null)
        {
            _logger.LogWarning(
                "Order not found when trying to remove item. OrderNumber {OrderNumber}",
                request.OrderNumber);

            throw new OrderNotFoundException($"Order '{request.OrderNumber}' not found.");
        }

        var item = order.Items
            .FirstOrDefault(i => i.ProductId == request.ProductId);

        if (item is null)
        {
            _logger.LogWarning(
                "Item not found in order when trying to remove. OrderNumber {OrderNumber}, ProductId {ProductId}",
                request.OrderNumber,
                request.ProductId);

            throw new OrderNotFoundException(
                $"Item with ProductId '{request.ProductId}' not found in order '{request.OrderNumber}'.");
        }
        var quantity = item.Quantity;

        _logger.LogInformation(
            "Removing item from order. OrderNumber {OrderNumber}, ProductId {ProductId}, Quantity {Quantity}",
            order.OrderNumber.Value,
            item.ProductId,
            quantity);

        order.UpdateItemQuantity(item, -quantity);

        await _uow.CommitAsync(ct);

        _logger.LogInformation(
            "RemoveOrderItemCommand completed successfully for OrderNumber {OrderNumber}. ItemsCount {ItemsCount}, Total {Total}",
            order.OrderNumber.Value,
            order.Items.Count,
            order.Total.Amount);

        return new RemoveOrderItemResponse
        {
            OrderNumber = order.OrderNumber.Value,
            ClientName = order.ClientName,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Total = order.Total.Amount,
            Items = order.Items.Select(i => new RemoveOrderItemResponseItem
            {
                ProductId = i.ProductId,
                Description = i.Description,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice.Amount,
                Subtotal = i.Subtotal.Amount
            }).ToList()
        };
    }
}
