using System.Text.Json.Serialization;

namespace ExampleApi.Dtos;

/// <summary>The issued JWT and its UTC expiry. camelCase field names per the contract.</summary>
public sealed class TokenResponse
{
    [JsonPropertyName("token")]
    public required string Token { get; set; }

    [JsonPropertyName("expiresAt")]
    public required DateTime ExpiresAt { get; set; }
}
