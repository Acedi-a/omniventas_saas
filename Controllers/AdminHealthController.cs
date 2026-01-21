using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/admin/health")]
[Authorize(Roles = "SuperAdmin")]
public class AdminHealthController : ControllerBase
{
    private readonly AdminHealthService _healthService;

    public AdminHealthController(AdminHealthService healthService)
    {
        _healthService = healthService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _healthService.GetSummaryAsync();
        return Ok(summary);
    }

    [HttpGet("trends")]
    public async Task<IActionResult> GetTrends([FromQuery] int days = 14)
    {
        var trends = await _healthService.GetTrendsAsync(days);
        return Ok(trends);
    }
}
