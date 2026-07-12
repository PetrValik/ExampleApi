using ExampleApi.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ExampleApi.Common.Filters;

/// <summary>
/// Global MVC exception filter that maps domain exceptions to RFC 7807 problem+json responses:
/// <see cref="NotFoundException"/> → 404, <see cref="ConflictException"/> → 409, anything else → 500.
/// </summary>
public sealed class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var (status, title) = context.Exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error"),
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(context.Exception, "Unhandled exception processing request.");
        }

        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{status}",
            Title = title,
            Status = status,
            Detail = status == StatusCodes.Status500InternalServerError
                ? "An unexpected error occurred."
                : context.Exception.Message,
        };

        context.Result = new ObjectResult(problemDetails)
        {
            StatusCode = status,
            ContentTypes = { "application/problem+json" },
        };

        context.ExceptionHandled = true;
    }
}
