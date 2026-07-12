using ExampleApi.Common.Results;
using MediatR;

namespace ExampleApi.Features.Auth.GetToken;

/// <summary>
/// Issues a JWT for the given credentials. Failure (invalid credentials) maps to 401.
/// </summary>
/// <param name="Username">The supplied username.</param>
/// <param name="Password">The supplied password.</param>
public sealed record GetTokenCommand(string Username, string Password)
    : IRequest<Result<GetTokenResponse>>;
