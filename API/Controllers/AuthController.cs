using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UrlShortner.API.Services;
using UrlShortner.Models;

namespace UrlShortner.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);

            if (response == null)
            {
                return BadRequest(new
                {
                    error = "Invalid registration data",
                    message = "Email format is invalid or password is too weak (minimum 8 characters)"
                });
            }

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = "Registration failed", message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);

        if (response == null)
        {
            return Unauthorized(new { error = "Invalid credentials", message = "Email or password is incorrect" });
        }

        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request.RefreshToken);

        if (response == null)
        {
            return Unauthorized(
                new { error = "Invalid refresh token", message = "Refresh token is invalid or expired" });
        }

        return Ok(response);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { error = "Invalid token", message = "User ID not found in token" });
        }

        var success = await _authService.LogoutAsync(userId);

        if (!success)
        {
            return BadRequest(new { error = "Logout failed", message = "User not found" });
        }

        return Ok(new { message = "Logged out successfully" });
    }
}