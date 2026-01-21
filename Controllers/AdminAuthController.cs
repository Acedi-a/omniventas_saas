using Microsoft.AspNetCore.Mvc;
using SaaSEventos.DTOs.Admin;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminAuthController : ControllerBase
{
    private readonly AdminAuthService _adminAuthService;

    public AdminAuthController(AdminAuthService adminAuthService)
    {
        _adminAuthService = adminAuthService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(AdminLoginRequest request)
    {
        try
        {
            var response = await _adminAuthService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}
