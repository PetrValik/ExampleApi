using ExampleApi.Dtos;
using ExampleApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExampleApi.Controllers;

/// <summary>
/// JWT token issuance. Anonymous.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("auth")]
public sealed class AuthController(ITokenService tokenService) : ControllerBase
{
    /// <summary>
    /// POST /auth/token → 200 with a signed JWT for valid demo credentials, otherwise 401.
    /// </summary>
    [HttpPost("token")]
    public IActionResult GetToken([FromBody] TokenRequest request)
    {
        var response = tokenService.Authenticate(request);
        return response is null ? Unauthorized() : Ok(response);
    }
}
