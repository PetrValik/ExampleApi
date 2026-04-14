using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExampleApi.Common.Endpoints;
using ExampleApi.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ExampleApi.Features.Auth;

/// <summary>
/// Endpoint for issuing JWT tokens.
/// </summary>
/// <remarks>
/// This uses hardcoded demo credentials (admin / admin).
/// Replace with a real identity store before going to production.
/// </remarks>
public sealed class TokenEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/token", (
            TokenRequest request,
            IOptions<JwtSettings> jwtOptions) =>
        {
            // Demo check — swap for real user validation in production
            if (request.Username != "admin" || request.Password != "admin")
            {
                return Results.Unauthorized();
            }

            var settings = jwtOptions.Value;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddMinutes(settings.ExpirationMinutes);

            var token = new JwtSecurityToken(
                issuer: settings.Issuer,
                audience: settings.Audience,
                claims: [new Claim(ClaimTypes.Name, request.Username)],
                expires: expiration,
                signingCredentials: credentials);

            return Results.Ok(new
            {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiresAt = expiration
            });
        })
        .WithName("GetToken")
        .WithTags("Auth")
        .WithSummary("Get JWT token")
        .WithDescription("Returns a JWT token. Demo credentials: username=admin, password=admin. Replace with a real identity provider in production.")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .AllowAnonymous();
    }
}
