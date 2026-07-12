namespace ExampleApi.Features.Auth.GetToken;

/// <summary>
/// Request body for obtaining a JWT token.
/// </summary>
/// <param name="Username">The user name to authenticate.</param>
/// <param name="Password">The password to authenticate.</param>
public sealed record GetTokenRequest(string Username, string Password);
