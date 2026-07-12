using System.Text.Json.Serialization;

namespace ExampleApi.Application.Auth.Dtos;

/// <summary>
/// Credentials submitted to the token endpoint.
/// </summary>
public sealed class GetTokenRequest
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}
