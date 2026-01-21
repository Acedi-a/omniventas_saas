using Microsoft.AspNetCore.Mvc;
using SaaSEventos.DTOs.Client;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/client/auth")]
public class ClientAuthController : ControllerBase
{
    private readonly ClientAuthService _clientAuthService;

    public ClientAuthController(ClientAuthService clientAuthService)
    {
        _clientAuthService = clientAuthService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(ClientRegisterRequest request)
    {
        try
        {
            var response = await _clientAuthService.RegisterAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(ClientLoginRequest request)
    {
        try
        {
            var response = await _clientAuthService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }
}
