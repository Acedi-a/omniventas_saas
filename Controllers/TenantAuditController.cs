using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/tenant/audit")]
[Authorize(Roles = "Admin,Validator")]
public class TenantAuditController : ControllerBase
{
    private readonly AuditQueryService _auditQueryService;

    public TenantAuditController(AuditQueryService auditQueryService)
    {
        _auditQueryService = auditQueryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? action,
        [FromQuery] int? userId,
        [FromQuery] string? metadata,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > 100)
        {
            pageSize = 20;
        }

        var logs = await _auditQueryService.GetTenantLogsAsync(tenantId, action, userId, metadata, from, to, page, pageSize);
        return Ok(logs);
    }

    private bool TryGetTenantId(out int tenantId)
    {
        var claim = User.FindFirst("TenantId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out tenantId);
    }
}
