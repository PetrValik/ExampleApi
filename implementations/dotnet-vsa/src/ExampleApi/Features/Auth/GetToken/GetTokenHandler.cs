using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExampleApi.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ExampleApi.Features.Auth.GetToken;

/// <summary>
/// Issues signed JWT tokens for authenticated users.
/// </summary>
/// <remarks>
/// This uses hardcoded demo credentials (<c>admin</c> / <c>admin</c>) as a stand-in for a real
/// identity provider. Replace <see cref="Handle"/> with a call into a real user store before
/// going to production. A real implementation would typically become asynchronous.
/// </remarks>
/// <param name="jwtOptions">The configured JWT settings.</param>
public sealed class GetTokenHandler(IOptions<JwtSettings> jwtOptions) : IGetTokenHandler
{
    private const string DemoUsername = "admin";
    private const string DemoPassword = "admin";

    /// <inheritdoc />
    public TokenResponse? Handle(TokenRequest request)
    {
        // Demo credential check — swap for real user validation in production.
        if (!string.Equals(request.Username, DemoUsername, StringComparison.Ordinal)
            || !string.Equals(request.Password, DemoPassword, StringComparison.Ordinal))
        {
            return null;
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
        return new TokenResponse(tokenString, expiresAt);
    }
}
