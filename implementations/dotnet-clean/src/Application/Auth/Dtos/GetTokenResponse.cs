using System.Text.Json.Serialization;

namespace ExampleApi.Application.Auth.Dtos;

/// <summary>
/// The issued token and its UTC expiry (camelCase per the contract).
/// </summary>
/// <param name="Token">The signed HS256 JWT.</param>
/// <param name="ExpiresAt">The UTC instant at which the token expires.</param>
public sealed record GetTokenResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("expiresAt")] DateTime ExpiresAt);
