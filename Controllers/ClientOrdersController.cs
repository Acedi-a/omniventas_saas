using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/client/orders")]
[Authorize(Roles = "Admin")]
public class ClientOrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public ClientOrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders()
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        var orders = await _orderService.GetTenantOrdersAsync(tenantId);
        return Ok(orders);
    }

    private bool TryGetTenantId(out int tenantId)
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        return int.TryParse(tenantIdClaim, out tenantId);
    }
}
