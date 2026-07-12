using System.Text.Json.Serialization;

namespace ExampleApi.Features.Auth.GetToken;

/// <summary>
/// The <c>POST /auth/token</c> response body (camelCase per the contract).
/// </summary>
/// <param name="Token">The signed HS256 JWT.</param>
/// <param name="ExpiresAt">The UTC expiry instant.</param>
public sealed record GetTokenResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("expiresAt")] DateTime ExpiresAt);
