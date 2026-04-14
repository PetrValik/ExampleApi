using ExampleApi.Common.Endpoints;

namespace ExampleApi.Features.Health;

/// <summary>
/// Health check endpoint for API monitoring.
/// </summary>
public sealed class HealthEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .WithName("HealthCheck")
            .WithTags("Health")
            .WithSummary("Health check")
            .WithDescription("Returns the health status of the API.")
            .Produces<object>(StatusCodes.Status200OK)
            .AllowAnonymous();
    }
}
