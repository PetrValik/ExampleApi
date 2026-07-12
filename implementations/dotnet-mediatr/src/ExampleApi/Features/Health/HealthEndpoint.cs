using ExampleApi.Common.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ExampleApi.Features.Health;

/// <summary>
/// <c>GET /health</c> — anonymous liveness probe returning <c>{"status":"healthy"}</c>.
/// </summary>
internal sealed class HealthEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .WithName("GetHealth")
            .WithTags("Health")
            .AllowAnonymous();
    }
}
