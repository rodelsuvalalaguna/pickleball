using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pickleball.Api.DTOs;
using Pickleball.Api.Services;

namespace Pickleball.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public class AuthController(AuthService auth) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterDto dto) => await auth.RegisterAsync(dto);

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginDto dto) => await auth.LoginAsync(dto);

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshDto dto) => await auth.RefreshAsync(dto.RefreshToken);

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshDto dto)
    {
        await auth.LogoutAsync(dto.RefreshToken);
        return NoContent();
    }
}
