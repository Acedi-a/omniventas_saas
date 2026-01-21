using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/analytics")]
[Authorize(Roles = "Admin")]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsService _analyticsService;

    public AnalyticsController(AnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("sales")]
    public async Task<IActionResult> GetSales([FromQuery] string? period)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        var stats = await _analyticsService.GetSalesStatsAsync(tenantId, period);
        return Ok(stats);
    }

    [HttpGet("top-products")]
    public async Task<IActionResult> GetTopProducts([FromQuery] int limit = 5)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        if (limit <= 0)
        {
            limit = 5;
        }

        var products = await _analyticsService.GetTopProductsAsync(tenantId, limit);
        return Ok(products);
    }

    [HttpGet("events/{id:int}/attendance")]
    public async Task<IActionResult> GetEventAttendance(int id)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        var attendance = await _analyticsService.GetEventAttendanceAsync(tenantId, id);
        if (attendance == null)
        {
            return NotFound(new { error = "Event not found." });
        }

        return Ok(attendance);
    }

    private bool TryGetTenantId(out int tenantId)
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        return int.TryParse(tenantIdClaim, out tenantId);
    }
}
