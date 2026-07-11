namespace ExampleApi.Features.Auth;

/// <summary>
/// Request body for obtaining a JWT token.
/// </summary>
public sealed record TokenRequest(string Username, string Password);
