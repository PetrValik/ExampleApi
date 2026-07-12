using Microsoft.AspNetCore.Routing;

namespace ExampleApi.Common.Endpoints;

/// <summary>
/// A self-registering Minimal-API endpoint module. Each vertical slice implements this
/// to map its own route(s); modules are discovered by reflection at startup.
/// </summary>
public interface IEndpoint
{
    /// <summary>Maps this slice's route(s) onto the application.</summary>
    void MapEndpoint(IEndpointRouteBuilder app);
}
