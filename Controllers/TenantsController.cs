using Microsoft.AspNetCore.Mvc;
using SaaSEventos.DTOs.Tenants;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/tenants")]
public class TenantsController : ControllerBase
{
    private readonly TenantService _tenantService;

    public TenantsController(TenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateTenant(CreateTenantRequest request)
    {
        var response = await _tenantService.CreateTenantAsync(request);
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTenant(int id)
    {
        var tenant = await _tenantService.GetTenantAsync(id);
        if (tenant == null)
        {
            return NotFound(new { error = "Tenant not found." });
        }

        return Ok(tenant);
    }
}
