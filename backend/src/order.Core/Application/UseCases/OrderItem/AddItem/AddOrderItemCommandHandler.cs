using MediatR;
using Order.Core.Domain.Orders.ValueObjects;
using Order.Core.Application.Abstractions.Repositories;
using OrdItem =Order.Core.Domain.Orders.OrderItem;
using Order.Core.Application.Abstractions;
using Order.Core.Domain.Orders.Enums;
using Order.Core.Application.Common.Exceptions;

namespace Order.Core.Application.UseCases.OrderItem.AddItem;

public class AddOrderItemCommandHandler
    : IRequestHandler<AddOrderItemCommand, AddOrderItemResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderItemRepository _itemRepository;
    private readonly IUnitOfWork _uow;

    public AddOrderItemCommandHandler(IOrderRepository orderRepository, IOrderItemRepository itemRepository, IUnitOfWork uow)
    {
        _orderRepository = orderRepository;
        _itemRepository = itemRepository;
        _uow = uow;
    }

    public async Task<AddOrderItemResponse> Handle(
        AddOrderItemCommand request,
        CancellationToken ct)
    {
        var order = await _orderRepository.GetByOrderNumberAsync(request.OrderNumber);
        if (order is null)
            throw new OrderNotFoundException($"Order '{request.OrderNumber}' not found.");

        if (order.Status == OrderStatus.Closed)
        {
            throw new InvalidOperationException(
                $"Cannot add items to a closed order. OrderNumber: {order.OrderNumber.Value}");
        }
        var unitPrice = Money.FromDecimal(request.UnitPrice);

        var existingItem = order.Items
            .FirstOrDefault(i =>
                i.Description == request.Description &&
                i.UnitPrice.Equals(unitPrice));

        if (existingItem is not null)
        {
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
            order.AddItem(newItem);
            await _itemRepository.AddAsync(newItem);
        }

        await _uow.CommitAsync(ct);


        return new AddOrderItemResponse
        {
            OrderNumber = order.OrderNumber.Value,
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
