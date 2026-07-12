using System.Text.Json.Serialization;

namespace ExampleApi.Dtos;

/// <summary>Credentials submitted to obtain a JWT.</summary>
public sealed class TokenRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}
