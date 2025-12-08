using MediatR;
using Order.Core.Application.Abstractions;
using Order.Core.Application.Abstractions.Repositories;
using Order.Core.Application.Common.Exceptions;
using Order.Core.Domain.Orders.Enums;

namespace Order.Core.Application.UseCases.Orders.CloseOrder;

public sealed class CloseOrderCommandHandler 
    : IRequestHandler<CloseOrderCommand, CloseOrderResponse>
{
    private readonly IOrderRepository _repository;
    private readonly IUnitOfWork _uow;

    public CloseOrderCommandHandler(IOrderRepository repository, IUnitOfWork uow)
    {
        _repository = repository;
        _uow = uow;
    }

    public async Task<CloseOrderResponse> Handle(CloseOrderCommand request, CancellationToken ct = default)
    {
        var order = await _repository.GetByOrderNumberAsync(request.OrderNumber, ct);
        if (order is null)
            throw new OrderNotFoundException($"Id do pedido: {request.OrderNumber}");

        order.Close();

        await _uow.CommitAsync(ct);

        return new CloseOrderResponse(
            order.OrderNumber.Value,
            order.Status.ToString(),
            order.CreatedAt,
            order.UpdatedAt,
            order.ClosedAt,
            order.Total.Amount
        );
    }
}
