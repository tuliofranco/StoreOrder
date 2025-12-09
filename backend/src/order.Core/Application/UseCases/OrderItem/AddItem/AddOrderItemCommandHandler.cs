using MediatR;
using Order.Core.Domain.Orders.ValueObjects;
using Order.Core.Application.Abstractions.Repositories;
using OrdItem =Order.Core.Domain.Orders.OrderItem;
using Order.Core.Application.Abstractions;
using Order.Core.Domain.Orders.Enums;
using Order.Core.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;


namespace Order.Core.Application.UseCases.OrderItem.AddItem;

public class AddOrderItemCommandHandler
    : IRequestHandler<AddOrderItemCommand, AddOrderItemResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _itemRepository;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<AddOrderItemCommandHandler> _logger;


    public AddOrderItemCommandHandler(IOrderRepository orderRepository, IOrderItemRepository itemRepository, IUnitOfWork uow, ILogger<AddOrderItemCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _itemRepository = itemRepository;
        _uow = uow;
        _logger = logger;
    }

    public async Task<AddOrderItemResponse> Handle(
        AddOrderItemCommand request,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Handling AddOrderItemCommand for OrderNumber {OrderNumber}, Description {Description}, Quantity {Quantity}, UnitPrice {UnitPrice}",
            request.OrderNumber,
            request.Description,
            request.Quantity,
            request.UnitPrice);

        var order = await _orderRepository.GetByOrderNumberAsync(request.OrderNumber);
        if (order is null)
        {
            _logger.LogWarning(
                "Order not found when trying to add item. OrderNumber {OrderNumber}",
                request.OrderNumber);

            throw new OrderNotFoundException($"Order '{request.OrderNumber}' not found.");
        }

        var unitPrice = Money.FromDecimal(request.UnitPrice);

        var existingItem = order.Items
            .FirstOrDefault(i =>
                i.Description == request.Description &&
                i.UnitPrice.Equals(unitPrice));

        if (existingItem is not null)
        {
            _logger.LogInformation(
                "Updating quantity of existing item in order. OrderNumber {OrderNumber}, ProductId {ProductId}, NewQuantity {Quantity}",
                order.OrderNumber.Value,
                existingItem.ProductId,
                request.Quantity);

            order.UpdateItemQuantity(existingItem, request.Quantity);
        }
        else
        {
            var newItem = OrdItem.Create(
                order.Id,
                request.Description,
                unitPrice,
                request.Quantity
            );

            _logger.LogInformation(
                "Adding new item to order. OrderNumber {OrderNumber}, ProductId {ProductId}, Description {Description}",
                order.OrderNumber.Value,
                newItem.ProductId,
                newItem.Description);

            order.AddItem(newItem);
            await _itemRepository.AddAsync(newItem);
        }

        await _uow.CommitAsync(ct);

        _logger.LogInformation(
            "AddOrderItemCommand completed successfully for OrderNumber {OrderNumber}. ItemsCount {ItemsCount}, Total {Total}",
            order.OrderNumber.Value,
            order.Items.Count,
            order.Total.Amount);
            
        return new AddOrderItemResponse
        {
            OrderNumber = order.OrderNumber.Value,
            ClientName = order.ClientName,
            Status = order.Status.ToString(),
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Total = order.Total.Amount,

            Items = order.Items.Select(i => new AddOrderItemResponseItem
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
