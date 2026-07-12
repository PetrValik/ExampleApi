namespace ExampleApi.WebApi.Endpoints;

/// <summary>Liveness endpoint (anonymous).</summary>
public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .WithName("GetHealth")
            .WithTags("Health")
            .AllowAnonymous();

        return app;
    }
}
