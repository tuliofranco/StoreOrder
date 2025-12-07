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
    public async Task<IActionResult> CreateOrder(CancellationToken ct)
    {
        try
        {
            var result = await _mediator.Send(new CreateOrderCommand(), ct);
            return Ok(new ResultViewModel<CreateOrderResponse>(result));
        }
        catch (OperationCanceledException)
        {
            return StatusCode(
                499,
                new ResultViewModel<CreateOrderResponse>(ErrorCatalog.Orders.Create_Cancelled)
            );
        }
        catch
        {
            return StatusCode(
                500,
                new ResultViewModel<CreateOrderResponse>(ErrorCatalog.Orders.Create_InternalError)
            );
        }
    }

    [HttpPost("{orderNumber}/addItem")]
    public async Task<IActionResult> AddOrderItem(
        string orderNumber,
        [FromBody] AddOrderItemRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.GetErrors();
            errors.Insert(0, ErrorCatalog.Orders.AddItem_Validation);
            return BadRequest(new ResultViewModel<AddOrderItemResponse?>(errors));
        }

        try
        {
            var command = new AddOrderItemCommand(
                orderNumber,
                request.Description,
                request.UnitPrice,
                request.Quantity
            );

            var result = await _mediator.Send(command, ct);

            return Ok(new ResultViewModel<AddOrderItemResponse?>(result));
        }
        catch (ValidationException ex)
        {
            var errors = ModelState.GetErrors();
            errors.Add(ex.Message);
            return BadRequest(new ResultViewModel<AddOrderItemResponse?>(errors));
        }
        catch (KeyNotFoundException ex)
        {
            var errors = new List<string>
            {
                $"{ErrorCatalog.Orders.GetOrder_NotFound} - {ex.Message}"
            };

            return NotFound(new ResultViewModel<AddOrderItemResponse?>(errors));
        }
        catch (InvalidOperationException ex)
        {
            var errors = new List<string>
            {
                ex.Message
            };

            return BadRequest(new ResultViewModel<AddOrderItemResponse?>(errors));
        }
        catch (OperationCanceledException)
        {
            return StatusCode(
                499,
                new ResultViewModel<AddOrderItemResponse?>(
                    ErrorCatalog.Orders.AddItem_Cancelled
                )
            );
        }
        catch
        {
            return StatusCode(
                500,
                new ResultViewModel<AddOrderItemResponse?>(
                    ErrorCatalog.Orders.AddItem_InternalError
                )
            );
        }
    }

    [HttpGet("{orderNumber}")]
    public async Task<IActionResult> GetOrder(string orderNumber, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<GetOrderByOrderNumberResponse>(ModelState.GetErrors()));
        try
        {
            var result = await _mediator.Send(new GetOrderByOrderNumberQuery(orderNumber), ct);
            return Ok(new ResultViewModel<GetOrderByOrderNumberResponse>(result));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ResultViewModel<GetOrderByOrderNumberResponse>(
                ErrorCatalog.Orders.GetOrder_NotFound
            ));
        }
        catch (OperationCanceledException)
        {
            return StatusCode(
                499,
                new ResultViewModel<GetOrderByOrderNumberResponse>(
                    ErrorCatalog.Orders.GetOrder_Cancelled
                )
            );
        }
        catch
        {
            return StatusCode(
                500,
                new ResultViewModel<GetOrderByOrderNumberResponse>(
                    ErrorCatalog.Orders.GetOrder_InternalError
                )
            );
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOrders(
            CancellationToken ct,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 25)
            
    {
        try
        {
            var query = new GetAllOrdersQuery(page, pageSize);
            var result = await _mediator.Send(query, ct);

            return Ok(new ResultViewModel<PagedResult<GetAllOrdersResponse>>(result));
        }
        catch (OperationCanceledException)
        {
            return StatusCode(
                499,
                new ResultViewModel<PagedResult<GetAllOrdersResponse>>(
                    ErrorCatalog.Orders.GetAll_Cancelled
                )
            );
        }
        catch
        {
            return StatusCode(
                500,
                new ResultViewModel<PagedResult<GetAllOrdersResponse>>(
                    ErrorCatalog.Orders.GetAll_InternalError
                )
            );
        }
    }

    [HttpPatch("{orderNumber}")]
    public async Task<IActionResult> CloseOrder(
        string orderNumber,
        CancellationToken ct)
    {

        try
        {
            var command = new CloseOrderCommand(orderNumber);
            var result = await _mediator.Send(command, ct);
            return Ok(new ResultViewModel<CloseOrderResponse>(result));
        }
        catch (ValidationException ex)
        {
            var errors = ModelState.GetErrors();
            errors.Add(ex.Message);
            return BadRequest(new ResultViewModel<CloseOrderResponse>(errors));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ResultViewModel<CloseOrderResponse>(
                ErrorCatalog.Orders.Close_NotFound
            ));
        }
        catch (InvalidOperationException ex)
        {
            var errors = new List<string>
            {
                ErrorCatalog.Orders.Close_BusinessRule,
                ex.Message
            };

            return BadRequest(new ResultViewModel<CloseOrderResponse>(errors));
        }
        catch (OperationCanceledException)
        {
            return StatusCode(
                499,
                new ResultViewModel<CloseOrderResponse>(
                    ErrorCatalog.Orders.Close_Cancelled
                )
            );
        }
        catch
        {
            return StatusCode(
                500,
                new ResultViewModel<CloseOrderResponse>(
                    ErrorCatalog.Orders.Close_InternalError
                )
            );
        }
    }


    [HttpDelete("{orderNumber}/{productId}")]
    public async Task<IActionResult> RemoveItem(
        string orderNumber,
        string productId,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(new ResultViewModel<RemoveOrderItemResponse>(ModelState.GetErrors()));

        try
        {
            var command = new RemoveOrderItemCommand(
                orderNumber,
                productId
            );

            var result = await _mediator.Send(command, ct);

            return Ok(new ResultViewModel<RemoveOrderItemResponse>(result));
        }
        catch (ValidationException ex)
        {
            var errors = ModelState.GetErrors();
            errors.Add(ex.Message);
            return BadRequest(new ResultViewModel<RemoveOrderItemResponse>(errors));
        }
        catch (KeyNotFoundException)
        {
            return NotFound(new ResultViewModel<RemoveOrderItemResponse>(
                ErrorCatalog.Orders.RemoveItem_NotFound
            ));
        }
        catch (InvalidOperationException ex)
        {
            var errors = new List<string>
            {
                ErrorCatalog.Orders.RemoveItem_BusinessRule,
                ex.Message
            };

            return BadRequest(new ResultViewModel<RemoveOrderItemResponse>(errors));
        }
        catch (OperationCanceledException)
        {
            return StatusCode(
                499,
                new ResultViewModel<RemoveOrderItemResponse>(
                    ErrorCatalog.Orders.RemoveItem_Cancelled
                )
            );
        }
        catch
        {
            return StatusCode(
                500,
                new ResultViewModel<RemoveOrderItemResponse>(
                    ErrorCatalog.Orders.RemoveItem_InternalError
                )
            );
        }
    }
}