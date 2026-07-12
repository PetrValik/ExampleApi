using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExampleApi.Controllers;

/// <summary>
/// Liveness endpoint. Anonymous.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    /// <summary>GET /health → 200 {"status":"healthy"}.</summary>
    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy" });
}
