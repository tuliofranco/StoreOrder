using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Order.Api.Errors;
using Order.Api.ViewModels;
using Order.Core.Application.Common.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Order.Api.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

#pragma warning disable MA0051 // Method is too long
    public async ValueTask<bool> TryHandleAsync(
#pragma warning restore MA0051 // Method is too long
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.ContentType = "application/json";

        var path = httpContext.Request.Path.Value ?? string.Empty;
        ResultViewModel<object> response;

        switch (exception)
        {
            // Validation Erros
            case OrderValidationException orderValidation:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                _logger.LogWarning(
                    exception,
                    "Validation error on path {Path}. ErrorCount: {ErrorCount}",
                    path,
                    orderValidation.Errors.Count());

                response = new ResultViewModel<object>(orderValidation.Errors.ToList());
                break;

            case ValidationException validation:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                _logger.LogWarning(
                    exception,
                    "ValidationException on path {Path}. Message: {Message}",
                    path,
                    validation.Message);

                response = new ResultViewModel<object>(new List<string> { validation.Message });
                break;

            //  Not Found
            case OrderNotFoundException notFound:
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

                _logger.LogWarning(
                    exception,
                    "OrderNotFoundException on path {Path}. Message: {Message}",
                    path,
                    notFound.Message);

                response = new ResultViewModel<object>(
                    new List<string> { notFound.Message }
                );
                break;

            case KeyNotFoundException keyNotFound:
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

                _logger.LogWarning(
                    exception,
                    "KeyNotFoundException on path {Path}. Message: {Message}",
                    path,
                    keyNotFound.Message);

                response = new ResultViewModel<object>(
                    new List<string> { keyNotFound.Message }
                );
                break;

            //  Business Rule Err
            case OrderBusinessException business:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                _logger.LogWarning(
                    exception,
                    "Business rule error on path {Path}. Message: {Message}",
                    path,
                    business.Message);

                response = new ResultViewModel<object>(
                    new List<string> { business.Message }
                );
                break;

            case InvalidOperationException invalidOp:
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

                _logger.LogWarning(
                    exception,
                    "InvalidOperationException on path {Path}. Message: {Message}",
                    path,
                    invalidOp.Message);

                response = new ResultViewModel<object>(
                    new List<string> { invalidOp.Message }
                );
                break;


            case OperationCanceledException:
                httpContext.Response.StatusCode = 499;

                _logger.LogInformation(
                    exception,
                    "Request was cancelled on path {Path}",
                    path);

                response = new ResultViewModel<object>(ErrorCatalog.Orders.GetAllCancelled);
                break;

            // Internal Server err
            default:
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

                _logger.LogError(
                    exception,
                    "Unhandled exception on path {Path}. Returning internal error code {ErrorCode}",
                    path,
                    ErrorCatalog.Orders.GetAllInternalError);

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
