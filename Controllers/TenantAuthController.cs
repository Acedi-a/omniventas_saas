using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SaaSEventos.DTOs.TenantAuth;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/tenant")]
public class TenantAuthController : ControllerBase
{
    private readonly TenantAuthService _authService;
    private readonly PasswordResetService _passwordResetService;

    public TenantAuthController(TenantAuthService authService, PasswordResetService passwordResetService)
    {
        _authService = authService;
        _passwordResetService = passwordResetService;
    }

    [HttpPost("login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(TenantLoginRequest request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpPost("password/forgot")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword(PasswordForgotRequest request)
    {
        try
        {
            var response = await _passwordResetService.RequestResetAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpPost("password/reset")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword(PasswordResetRequest request)
    {
        try
        {
            await _passwordResetService.ResetAsync(request);
            return Ok(new { success = true });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}
