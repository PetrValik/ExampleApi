using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExampleApi.Configuration;
using ExampleApi.Dtos;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ExampleApi.Services;

/// <summary>
/// Issues HS256 JWTs for the hardcoded demo user (<c>admin</c> / <c>admin</c>).
/// Replace the credential check with a real identity provider in production.
/// </summary>
public sealed class TokenService(IOptions<JwtSettings> jwtOptions) : ITokenService
{
    private const string DemoUsername = "admin";
    private const string DemoPassword = "admin";

    /// <inheritdoc />
    public TokenResponse? Authenticate(TokenRequest request)
    {
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
        return new TokenResponse { Token = tokenString, ExpiresAt = expiresAt };
    }
}
