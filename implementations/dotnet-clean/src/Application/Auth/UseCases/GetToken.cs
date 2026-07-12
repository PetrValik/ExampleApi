using ExampleApi.Application.Abstractions;
using ExampleApi.Application.Auth.Dtos;

namespace ExampleApi.Application.Auth.UseCases;

/// <summary>Use case: exchange demo credentials for a signed bearer token.</summary>
public interface IGetTokenHandler
{
    /// <summary>
    /// Returns a token response for valid credentials, or <c>null</c> to signal 401.
    /// </summary>
    GetTokenResponse? Handle(GetTokenRequest request);
}

/// <inheritdoc />
public sealed class GetTokenHandler(ITokenService tokenService) : IGetTokenHandler
{
    // Demo credentials — documented stand-in for a real identity provider.
    private const string DemoUsername = "admin";
    private const string DemoPassword = "admin";

    public GetTokenResponse? Handle(GetTokenRequest request)
    {
        var credentialsValid =
            string.Equals(request.Username, DemoUsername, StringComparison.Ordinal) &&
            string.Equals(request.Password, DemoPassword, StringComparison.Ordinal);

        if (!credentialsValid)
        {
            return null;
        }

        var (token, expiresAt) = tokenService.GenerateToken(request.Username);
        return new GetTokenResponse(token, expiresAt);
    }
}
