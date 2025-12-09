using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "CreateOrder requested for ClientName {ClientName}",
            request.ClientName);

        var result = await _mediator.Send(new CreateOrderCommand(request.ClientName), ct);

        _logger.LogInformation(
            "CreateOrder succeeded with OrderNumber {OrderNumber}",
            result.OrderNumber);

        return Ok(new ResultViewModel<CreateOrderResponse>(result));
    }

    [HttpPost("{orderNumber}/addItem")]
    public async Task<IActionResult> AddOrderItem(
        string orderNumber,
        [FromBody] AddOrderItemRequest request,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);

        _logger.LogInformation(
            "AddOrderItem requested for OrderNumber {OrderNumber} with Description {Description}, Quantity {Quantity}",
            orderNumber,
            request.Description,
            request.Quantity);

        var command = new AddOrderItemCommand(
            orderNumber,
            request.Description,
            request.UnitPrice,
            request.Quantity
        );

        var result = await _mediator
            .Send(command, ct)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "AddOrderItem completed for OrderNumber {OrderNumber}. ItemId: {ItemId}",
            orderNumber,
            result?.Items.Select(i => i.ProductId));

        return Ok(new ResultViewModel<AddOrderItemResponse?>(result));
    }

    [HttpGet("{orderNumber}")]
    public async Task<IActionResult> GetOrder(string orderNumber, CancellationToken ct)
    {
        _logger.LogInformation(
            "GetOrder requested for OrderNumber {OrderNumber}",
            orderNumber);

        var result = await _mediator.Send(new GetOrderByOrderNumberQuery(orderNumber), ct);

        _logger.LogInformation(
            "GetOrder completed for OrderNumber {OrderNumber}",
            orderNumber);

        return Ok(new ResultViewModel<GetOrderByOrderNumberResponse>(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders(
        CancellationToken ct,
        [FromQuery] int page = 0,
        [FromQuery] int pageSize = 25,
        [FromQuery] OrderStatus? status = null)
    {
        _logger.LogInformation(
            "GetAllOrders requested. Page {Page}, PageSize {PageSize}, StatusFilter {Status}",
            page,
            pageSize,
            status?.ToString() ?? "null");

        var query = new GetAllOrdersQuery(page, pageSize, status);
        var result = await _mediator.Send(query, ct);

        _logger.LogInformation(
            "GetAllOrders completed. Page {Page}, ReturnedCount {Count}",
            page,result.Items.Count());

        return Ok(new ResultViewModel<PagedResult<GetAllOrdersResponse>>(result));
    }

    [HttpPatch("{orderNumber}")]
    public async Task<IActionResult> CloseOrder(
        string orderNumber,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "CloseOrder requested for OrderNumber {OrderNumber}",
            orderNumber);

        var command = new CloseOrderCommand(orderNumber);
        var result = await _mediator.Send(command, ct);

        _logger.LogInformation(
            "CloseOrder completed for OrderNumber {OrderNumber} with Status {Status}",
            orderNumber,
            result.Status);

        return Ok(new ResultViewModel<CloseOrderResponse>(result));
    }

    [HttpDelete("{orderNumber}/{productId}")]
    public async Task<IActionResult> RemoveItem(
        string orderNumber,
        string productId,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "RemoveItem requested. OrderNumber {OrderNumber}, ProductId {ProductId}",
            orderNumber,
            productId);

        var command = new RemoveOrderItemCommand(
            orderNumber,
            productId
        );

        var result = await _mediator.Send(command, ct);

        _logger.LogInformation(
            "RemoveItem completed. OrderNumber {OrderNumber}, ProductId {ProductId}",
            orderNumber,
            productId);

        return Ok(new ResultViewModel<RemoveOrderItemResponse>(result));
    }
}