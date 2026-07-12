using ExampleApi.Application.Auth.Dtos;
using ExampleApi.Application.Auth.UseCases;

namespace ExampleApi.WebApi.Endpoints;

/// <summary>Token issuance endpoint (anonymous).</summary>
public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/token", (GetTokenRequest request, IGetTokenHandler handler) =>
            {
                var response = handler.Handle(request);
                return response is null
                    ? Results.Unauthorized()
                    : Results.Ok(response);
            })
            .WithName("GetToken")
            .WithTags("Auth")
            .AllowAnonymous();

        return app;
    }
}
