using ExampleApi.Common.Endpoints;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ExampleApi.Features.Auth.GetToken;

/// <summary>
/// <c>POST /auth/token</c> — anonymous. Returns a JWT for valid credentials, else 401.
/// </summary>
internal sealed class GetTokenEndpoint : IEndpoint
{
    /// <inheritdoc />
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/token", async (
                GetTokenRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetTokenCommand(request.Username, request.Password), cancellationToken);

                return result.IsSuccess
                    ? Results.Ok(result.Value)
                    : Results.Unauthorized();
            })
            .WithName("GetToken")
            .WithTags("Auth")
            .AllowAnonymous();
    }
}
