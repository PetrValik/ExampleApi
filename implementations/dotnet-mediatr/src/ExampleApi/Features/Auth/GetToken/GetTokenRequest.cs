using System.Text.Json.Serialization;

namespace ExampleApi.Features.Auth.GetToken;

/// <summary>
/// The <c>POST /auth/token</c> request body.
/// </summary>
public sealed class GetTokenRequest
{
    /// <summary>The username (demo: <c>admin</c>).</summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;

    /// <summary>The password (demo: <c>admin</c>).</summary>
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}
