using System.Text.Json.Serialization;

namespace ExampleApi.Features.Auth.GetToken;

/// <summary>
/// Response returned by the token endpoint.
/// </summary>
/// <param name="Token">The signed JWT bearer token.</param>
/// <param name="ExpiresAt">The UTC instant at which the token expires.</param>
public sealed record GetTokenResponse(
    [property: JsonPropertyName("token")] string Token,
    [property: JsonPropertyName("expiresAt")] DateTime ExpiresAt);
