using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/owner/audit")]
[Authorize(Roles = "Owner")]
public class OwnerAuditController : ControllerBase
{
    private readonly AuditQueryService _auditQueryService;

    public OwnerAuditController(AuditQueryService auditQueryService)
    {
        _auditQueryService = auditQueryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? action,
        [FromQuery] int? tenantId,
        [FromQuery] int? userId,
        [FromQuery] string? metadata,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Unauthorized(new { error = "AccountId claim missing." });
        }

        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > 100)
        {
            pageSize = 20;
        }

        var logs = await _auditQueryService.GetOwnerLogsAsync(accountId, action, tenantId, userId, metadata, from, to, page, pageSize);
        return Ok(logs);
    }

    private bool TryGetAccountId(out int accountId)
    {
        var claim = User.FindFirst("AccountId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out accountId);
    }
}
