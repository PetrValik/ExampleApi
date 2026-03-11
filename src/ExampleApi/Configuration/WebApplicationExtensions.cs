using ExampleApi.Infrastructure.Database;
using Scalar.AspNetCore;

namespace ExampleApi.Configuration;

/// <summary>
/// Extension methods for configuring the application middleware pipeline.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Initializes the database schema and seed data.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        await app.Services.InitialiseDatabaseAsync();
    }

    /// <summary>
    /// Configures global exception handling middleware.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errApp => errApp.Run(async ctx =>
        {
            var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;

            (int status, string title) = ex switch
            {
                Common.Exceptions.NotFoundException => (404, "Not Found"),
                Common.Exceptions.ConflictException => (409, "Conflict"),
                _ => (500, "Internal Server Error")
            };

            if (status == 500 && ex is not null)
            {
                var logger = ctx.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("GlobalExceptionHandler");
                logger.LogError(ex, "Unhandled exception occurred");
            }

            ctx.Response.StatusCode = status;

            var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Type = $"https://httpstatuses.com/{status}",
                Title = title,
                Status = status,
                Detail = status == 500 ? "An unexpected error occurred." : ex?.Message
            };

            var problemDetailsService = ctx.RequestServices.GetService<IProblemDetailsService>();
            if (problemDetailsService is not null)
            {
                await problemDetailsService.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = ctx,
                    ProblemDetails = problemDetails
                });
            }
            else
            {
                await ctx.Response.WriteAsJsonAsync(problemDetails);
            }
        }));

        return app;
    }

    /// <summary>
    /// Configures API documentation (OpenAPI and Scalar UI) for Development environment.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseApiDocumentation(this WebApplication app, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        return app;
    }
}
