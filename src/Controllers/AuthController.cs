using System.Security.Claims;
using backend.src.DTOs;
using backend.src.Helpers;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Sign in an admin user and return access and refresh tokens.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Login successful"));
    }

    /// <summary>
    /// Rotate a valid refresh token and return a new token pair.
    /// </summary>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok(ApiResponse<AuthResponse>.Ok(result, "Token refreshed"));
    }

    /// <summary>
    /// Revoke a refresh token.
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Logout successful"));
    }

    /// <summary>
    /// Change the current admin password.
    /// </summary>
    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        await _authService.ChangePasswordAsync(GetUserId(), request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok(ApiResponse<object>.Ok(null, "Password changed"));
    }

    /// <summary>
    /// Enable TOTP two-factor authentication for the current admin.
    /// </summary>
    [Authorize]
    [HttpPost("enable-2fa")]
    [ProducesResponseType(typeof(ApiResponse<EnableTwoFactorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<EnableTwoFactorResponse>>> EnableTwoFactor(CancellationToken cancellationToken)
    {
        var result = await _authService.EnableTwoFactorAsync(GetUserId(), HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return Ok(ApiResponse<EnableTwoFactorResponse>.Ok(result, "Two-factor authentication enabled"));
    }

    /// <summary>
    /// Verify a TOTP two-factor code for the current admin.
    /// </summary>
    [Authorize]
    [HttpPost("verify-2fa")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<object>>> VerifyTwoFactor(VerifyTwoFactorRequest request, CancellationToken cancellationToken)
    {
        var isValid = await _authService.VerifyTwoFactorAsync(GetUserId(), request, HttpContext.Connection.RemoteIpAddress?.ToString(), cancellationToken);
        return isValid
            ? Ok(ApiResponse<object>.Ok(null, "Two-factor code verified"))
            : BadRequest(ApiResponse<object>.Fail("Invalid two-factor code"));
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("Missing user identity");
    }
}
