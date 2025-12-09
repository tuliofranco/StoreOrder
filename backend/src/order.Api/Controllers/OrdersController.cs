using MediatR;
using Microsoft.AspNetCore.Mvc;
using Order.Core.Application.UseCases.Orders.CreateOrder;
using Order.Core.Application.UseCases.Orders.GetOrderByOrderNumber;
using Order.Api.ViewModels;
using Order.Api.Extensions;
using Order.Core.Application.UseCases.Orders.GetAllOrders;
using Order.Core.Application.UseCases.OrderItem.AddItem;
using Order.Core.Application.UseCases.Orders.CloseOrder;
using Order.Core.Application.UseCases.OrderItem.RemoveItem;
using Order.Api.ViewModels.OrderItem;
using System.ComponentModel.DataAnnotations;
using Order.Api.Errors;
using Order.Core.Application.Common;
using Order.Core.Domain.Orders.Enums;
using Order.Api.ViewModels.Order;

namespace Order.Api.Controllers;

[ApiController]
[Route("api/v1/orders")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request ,CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var result = await _mediator.Send(new CreateOrderCommand(request.ClientName), ct);
        return Ok(new ResultViewModel<CreateOrderResponse>(result));
    }


    [HttpPost("{orderNumber}/addItem")]
    public async Task<IActionResult> AddOrderItem(
    string orderNumber,
    [FromBody] AddOrderItemRequest request,
    CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        var command = new AddOrderItemCommand(
            orderNumber,
            request.Description,
            request.UnitPrice,
            request.Quantity
        );

        var result = await _mediator
            .Send(command, ct)
            .ConfigureAwait(false);



        return Ok(new ResultViewModel<AddOrderItemResponse?>(result));
    }

    [HttpGet("{orderNumber}")]
    public async Task<IActionResult> GetOrder(string orderNumber, CancellationToken ct)
    {

        var result = await _mediator.Send(new GetOrderByOrderNumberQuery(orderNumber), ct);
        return Ok(new ResultViewModel<GetOrderByOrderNumberResponse>(result));

    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders(
            CancellationToken ct,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 25,
            [FromQuery] OrderStatus? status = null)

    {
        var query = new GetAllOrdersQuery(page, pageSize, status);
        var result = await _mediator.Send(query, ct);
        return Ok(new ResultViewModel<PagedResult<GetAllOrdersResponse>>(result));
        
    }

    [HttpPatch("{orderNumber}")]
    public async Task<IActionResult> CloseOrder(
        string orderNumber,
        CancellationToken ct)
    {
        var command = new CloseOrderCommand(orderNumber);
        var result = await _mediator.Send(command, ct);
        return Ok(new ResultViewModel<CloseOrderResponse>(result));
    }


    [HttpDelete("{orderNumber}/{productId}")]
    public async Task<IActionResult> RemoveItem(
        string orderNumber,
        string productId,
        CancellationToken ct)
    {

        var command = new RemoveOrderItemCommand(
                orderNumber,
                productId
            );

        var result = await _mediator.Send(command, ct);
        return Ok(new ResultViewModel<RemoveOrderItemResponse>(result));
    }
}