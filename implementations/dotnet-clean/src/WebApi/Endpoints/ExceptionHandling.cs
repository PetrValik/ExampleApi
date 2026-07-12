using ExampleApi.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ExampleApi.WebApi.Endpoints;

/// <summary>
/// Translates use-case exceptions into RFC 7807 <c>application/problem+json</c> responses:
/// <see cref="NotFoundException"/> → 404, <see cref="ConflictException"/> → 409, and any
/// other unhandled exception → 500.
/// </summary>
public static class ExceptionHandling
{
    public static IApplicationBuilder UseDomainExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
        {
            var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;

            var (status, title) = error switch
            {
                NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            if (status == StatusCodes.Status500InternalServerError && error is not null)
            {
                context.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("GlobalExceptionHandler")
                    .LogError(error, "Unhandled exception.");
            }

            context.Response.StatusCode = status;

            var problem = new ProblemDetails
            {
                Type = $"https://httpstatuses.com/{status}",
                Title = title,
                Status = status,
                Detail = status == StatusCodes.Status500InternalServerError
                    ? "An unexpected error occurred."
                    : error?.Message
            };

            // IProblemDetailsService is always registered via AddProblemDetails(); it emits
            // the body as application/problem+json.
            var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
            await problemDetailsService.WriteAsync(new ProblemDetailsContext
            {
                HttpContext = context,
                ProblemDetails = problem
            });
        }));

        return app;
    }
}
