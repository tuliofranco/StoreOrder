using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Order.Api.Errors;
using Order.Api.ViewModels;
using Order.Core.Application.Common.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Order.Api.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
#pragma warning disable MA0051 // Method is too long
    public async ValueTask<bool> TryHandleAsync(
#pragma warning restore MA0051 // Method is too long
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.ContentType = "application/json";

        ResultViewModel<object> response;

        switch (exception)
        {
            // Validation Erros
            case OrderValidationException orderValidation:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new ResultViewModel<object>(orderValidation.Errors.ToList());
                break;

            case ValidationException validation:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new ResultViewModel<object>(new List<string> { validation.Message });
                break;

            //  Not Found
            case OrderNotFoundException notFound:
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                response = new ResultViewModel<object>(
                    new List<string> { notFound.Message }
                );
                break;

            case KeyNotFoundException keyNotFound:
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                response = new ResultViewModel<object>(
                    new List<string> { keyNotFound.Message }
                );
                break;

            //  Business Rule Err
            case OrderBusinessException business:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new ResultViewModel<object>(
                    new List<string> { business.Message }
                );
                break;

            case InvalidOperationException invalidOp:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                response = new ResultViewModel<object>(
                    new List<string> { invalidOp.Message }
                );
                break;


            case OperationCanceledException:
                httpContext.Response.StatusCode = 499;
                response = new ResultViewModel<object>(ErrorCatalog.Orders.GetAllCancelled);
                break;

            // Internal Server err
            default:
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response = new ResultViewModel<object>(
                    ErrorCatalog.Orders.GetAllInternalError
                );
                break;
        }

        await httpContext.Response
            .WriteAsJsonAsync(response, cancellationToken)
            .ConfigureAwait(false);

        return true;
    }
}
