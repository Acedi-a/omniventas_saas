using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/client/tenant")]
[Authorize(Roles = "Admin")]
public class ClientTenantController : ControllerBase
{
    private readonly TenantService _tenantService;

    public ClientTenantController(TenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        var tenant = await _tenantService.GetTenantAsync(tenantId);
        if (tenant == null)
        {
            return NotFound(new { error = "Tenant not found." });
        }

        return Ok(tenant);
    }

    [HttpPost("regenerate-api-key")]
    public async Task<IActionResult> RegenerateApiKey()
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        var tenant = await _tenantService.RegenerateApiKeyAsync(tenantId);
        if (tenant == null)
        {
            return NotFound(new { error = "Tenant not found." });
        }

        return Ok(tenant);
    }

    private bool TryGetTenantId(out int tenantId)
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        return int.TryParse(tenantIdClaim, out tenantId);
    }
}
