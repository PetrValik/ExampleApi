using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExampleApi.Application.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ExampleApi.Infrastructure.Authentication;

/// <summary>
/// HS256 JWT adapter for <see cref="ITokenService"/>. Signs a token carrying a name
/// claim, issuer, audience and expiry taken from <see cref="JwtSettings"/>.
/// </summary>
public sealed class JwtTokenService(IOptions<JwtSettings> options) : ITokenService
{
    public (string Token, DateTime ExpiresAt) GenerateToken(string username)
    {
        var settings = options.Value;

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(settings.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: [new Claim(ClaimTypes.Name, username)],
            expires: expiresAt,
            signingCredentials: credentials);

        var serialized = new JwtSecurityTokenHandler().WriteToken(token);
        return (serialized, expiresAt);
    }
}
