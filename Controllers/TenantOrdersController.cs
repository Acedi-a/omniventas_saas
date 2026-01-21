using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/tenant/orders")]
[Authorize(Roles = "Admin,Validator")]
public class TenantOrdersController : ControllerBase
{
    private readonly TenantOrdersService _ordersService;

    public TenantOrdersController(TenantOrdersService ordersService)
    {
        _ordersService = ordersService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        var orders = await _ordersService.GetOrdersAsync(tenantId);
        return Ok(orders);
    }

    private bool TryGetTenantId(out int tenantId)
    {
        var claim = User.FindFirst("TenantId")?.Value;
        return int.TryParse(claim, out tenantId);
    }
}
