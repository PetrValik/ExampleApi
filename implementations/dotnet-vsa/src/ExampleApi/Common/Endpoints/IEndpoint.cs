namespace ExampleApi.Common.Endpoints;

/// <summary>
/// Defines a contract for API endpoint registration.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Maps the endpoint routes to the application.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    void MapEndpoint(IEndpointRouteBuilder app);
}