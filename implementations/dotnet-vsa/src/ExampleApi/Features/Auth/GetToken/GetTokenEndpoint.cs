using ExampleApi.Common.Endpoints;
using Microsoft.AspNetCore.Mvc;

namespace ExampleApi.Features.Auth.GetToken;

/// <summary>
/// Endpoint for issuing JWT tokens.
/// </summary>
public sealed class GetTokenEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/token", (
            GetTokenRequest request,
            IGetTokenHandler handler) =>
        {
            var response = handler.Handle(request);

            return response is null
                ? Results.Unauthorized()
                : Results.Ok(response);
        })
        .WithName("GetToken")
        .WithTags("Auth")
        .WithSummary("Get JWT token")
        .WithDescription("Returns a JWT token. Demo credentials: username=admin, password=admin. Replace with a real identity provider in production.")
        .Produces<GetTokenResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();
    }
}
