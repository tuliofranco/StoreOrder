using MediatR;
using Order.Core.Application.Abstractions.Repositories;

namespace Order.Core.Application.UseCases.GetOrderByOrderNumber;

public class GetOrderByOrderNumberQueryHandler : IRequestHandler<GetOrderByOrderNumberQuery, GetOrderByOrderNumberResponse>
{
    private readonly IOrderRepository _repository;

    public GetOrderByOrderNumberQueryHandler(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetOrderByOrderNumberResponse> Handle(GetOrderByOrderNumberQuery request, CancellationToken ct)
    {
        var order = await _repository.GetByIdAsync(request.OrderNumber, ct);

        if (order is null)
            throw new Exception("Order not found");

        return new GetOrderByOrderNumberResponse(
        order.OrderNumber.Value,
        order.Status.ToString(),
        order.CreatedAt,
        order.UpdatedAt,
        order.ClosedAt,
        order.Total.Amount
    );
    }
}
