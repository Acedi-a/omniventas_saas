using SaaSEventos.Data;
using SaaSEventos.Models;

namespace SaaSEventos.Services;

public class AuditService
{
    private readonly AppDbContext _db;

    public AuditService(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(string action, int? accountId = null, int? tenantId = null, int? userId = null, string? metadata = null)
    {
        var log = new AuditLog
        {
            Action = action,
            AccountId = accountId,
            TenantId = tenantId,
            UserId = userId,
            Metadata = metadata,
            CreatedAt = DateTime.UtcNow
        };

        _db.AuditLogs.Add(log);
        await _db.SaveChangesAsync();
    }
}
