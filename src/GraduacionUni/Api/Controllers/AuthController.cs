using GraduacionUni.Application.DTOs;
using GraduacionUni.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GraduacionUni.Api.Controllers;

[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ApiControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var response = await authService.LoginAsync(request, ct);

        return response is null
            ? Unauthorized(new { message = "Credenciales inválidas." })
            : Ok(response);
    }
}
