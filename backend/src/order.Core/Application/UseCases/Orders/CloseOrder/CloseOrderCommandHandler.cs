using MediatR;
using Order.Core.Application.Abstractions;
using Microsoft.Extensions.Logging;
using Order.Core.Application.Abstractions.Repositories;
using Order.Core.Application.Common.Exceptions;
using Order.Core.Domain.Orders.Enums;

namespace Order.Core.Application.UseCases.Orders.CloseOrder;

public sealed class CloseOrderCommandHandler 
    : IRequestHandler<CloseOrderCommand, CloseOrderResponse>
{
    private readonly IOrderRepository _repository;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<CloseOrderCommandHandler> _logger;

    public CloseOrderCommandHandler(IOrderRepository repository, IUnitOfWork uow, ILogger<CloseOrderCommandHandler> logger)
    {
        _repository = repository;
        _uow = uow;
        _logger = logger;
    }

    public async Task<CloseOrderResponse> Handle(CloseOrderCommand request, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Handling CloseOrderCommand for OrderNumber {OrderNumber}",
            request.OrderNumber);

        var order = await _repository.GetByOrderNumberAsync(request.OrderNumber, ct);

        if (order is null)
        {
            _logger.LogWarning(
                "Order not found when trying to close. OrderNumber {OrderNumber}",
                request.OrderNumber);

            throw new OrderNotFoundException($"Id do pedido: {request.OrderNumber}");
        }

        order.Close();

        await _uow.CommitAsync(ct);

        _logger.LogInformation(
            "Order closed successfully. OrderNumber {OrderNumber}, Status {Status}",
            order.OrderNumber.Value,
            order.Status);

        return new CloseOrderResponse(
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
