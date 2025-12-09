using MediatR;
using Order.Core.Application.Abstractions;
using Order.Core.Application.Abstractions.Repositories;
using Microsoft.Extensions.Logging;
using OrderEntity = Order.Core.Domain.Orders.Order;

namespace Order.Core.Application.UseCases.Orders.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IOrderRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CreateOrderCommandHandler> _logger;


    public CreateOrderCommandHandler(IOrderRepository repository, IUnitOfWork uow, ILogger<CreateOrderCommandHandler> logger)
    {
        _repository = repository;
        _uow = uow;
        _logger = logger;
    }

    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Handling CreateOrderCommand for ClientName {ClientName}",
            request.clientName);
            
        OrderEntity order = OrderEntity.Create(request.clientName);
        await _repository.AddAsync(order, ct);

        await _uow.CommitAsync();
        
        _logger.LogInformation(
            "Order created successfully. OrderNumber {OrderNumber}",
            order.OrderNumber.Value);

        return new CreateOrderResponse(
            order.OrderNumber.Value,
            order.ClientName,
            order.Status.ToString(),
            order.CreatedAt,
            order.UpdatedAt,
            order.ClosedAt,
            order.Total.Amount
        );
    }
}