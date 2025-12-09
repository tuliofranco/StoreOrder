using System.ComponentModel.DataAnnotations;
using MediatR;
using Order.Core.Application.Abstractions;
using Order.Core.Application.Abstractions.Repositories;
using Order.Core.Application.Common.Exceptions;
using Order.Core.Domain.Orders.Enums;
using Order.Core.Domain.Orders.ValueObjects;

namespace Order.Core.Application.UseCases.OrderItem.RemoveItem;

public class RemoveOrderItemCommandHandler
    : IRequestHandler<RemoveOrderItemCommand, RemoveOrderItemResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _uow;

    public RemoveOrderItemCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork uow)
    {
        _orderRepository = orderRepository;
        _uow = uow;
    }

    public async Task<RemoveOrderItemResponse> Handle(
        RemoveOrderItemCommand request,
        CancellationToken ct)
    {

        var order = await _orderRepository.GetByOrderNumberAsync(request.OrderNumber);

        if (order is null)
            throw new OrderNotFoundException($"Order '{request.OrderNumber}' not found.");

        var item = order.Items
            .FirstOrDefault(i => i.ProductId == request.ProductId);

        if (item is null)
        {
            throw new OrderNotFoundException(
                $"Item with ProductId '{request.ProductId}' not found in order '{request.OrderNumber}'.");
        }
        var quantity = item.Quantity;

        order.UpdateItemQuantity(item, -quantity);

        await _uow.CommitAsync(ct);

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
