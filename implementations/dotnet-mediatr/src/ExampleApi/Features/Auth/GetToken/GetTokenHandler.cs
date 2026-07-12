using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExampleApi.Common.Results;
using ExampleApi.Configuration;
using MediatR;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ExampleApi.Features.Auth.GetToken;

/// <summary>
/// Validates the demo credentials (<c>admin</c>/<c>admin</c>) and issues a signed HS256 JWT
/// with a name claim, issuer, audience and expiry taken from configuration.
/// </summary>
internal sealed class GetTokenHandler(IOptions<JwtSettings> jwtOptions)
    : IRequestHandler<GetTokenCommand, Result<GetTokenResponse>>
{
    private const string DemoUsername = "admin";
    private const string DemoPassword = "admin";

    /// <inheritdoc />
    public Task<Result<GetTokenResponse>> Handle(GetTokenCommand request, CancellationToken cancellationToken)
    {
        // Demo credential check — replace with a real user store in production.
        if (!string.Equals(request.Username, DemoUsername, StringComparison.Ordinal)
            || !string.Equals(request.Password, DemoPassword, StringComparison.Ordinal))
        {
            return Task.FromResult(
                Result.Failure<GetTokenResponse>(
                    Error.Failure("Auth.InvalidCredentials", "The supplied credentials are invalid.")));
        }

        var settings = jwtOptions.Value;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(settings.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: [new Claim(ClaimTypes.Name, request.Username)],
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return Task.FromResult(
            Result.Success(new GetTokenResponse(tokenString, expiresAt)));
    }
}
