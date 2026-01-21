using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/admin/audit")]
[Authorize(Roles = "SuperAdmin")]
public class AdminAuditController : ControllerBase
{
    private readonly AuditQueryService _auditQueryService;

    public AdminAuditController(AuditQueryService auditQueryService)
    {
        _auditQueryService = auditQueryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLogs(
        [FromQuery] string? action,
        [FromQuery] int? tenantId,
        [FromQuery] int? accountId,
        [FromQuery] int? userId,
        [FromQuery] string? metadata,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > 100)
        {
            pageSize = 20;
        }

        var logs = await _auditQueryService.GetAdminLogsAsync(action, tenantId, accountId, userId, metadata, from, to, page, pageSize);
        return Ok(logs);
    }
}
