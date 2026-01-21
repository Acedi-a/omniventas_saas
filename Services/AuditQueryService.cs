using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.Admin;

namespace SaaSEventos.Services;

public class AuditQueryService
{
    private readonly AppDbContext _db;

    public AuditQueryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TenantPagedResponse<AuditLogResponse>> GetAdminLogsAsync(
        string? action,
        int? tenantId,
        int? accountId,
        int? userId,
        string? metadata,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize)
    {
        var query = BuildQuery(action, tenantId, accountId, userId, metadata, from, to);
        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AuditLogResponse
            {
                Id = l.Id,
                Action = l.Action,
                AccountId = l.AccountId,
                TenantId = l.TenantId,
                UserId = l.UserId,
                Metadata = l.Metadata,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new TenantPagedResponse<AuditLogResponse>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<TenantPagedResponse<AuditLogResponse>> GetOwnerLogsAsync(
        int accountId,
        string? action,
        int? tenantId,
        int? userId,
        string? metadata,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize)
    {
        var tenantIds = await _db.Tenants
            .Where(t => t.AccountId == accountId)
            .Select(t => t.Id)
            .ToListAsync();

        var query = BuildQuery(action, tenantId, accountId, userId, metadata, from, to)
            .Where(l => (l.AccountId != null && l.AccountId == accountId) || (l.TenantId != null && tenantIds.Contains(l.TenantId.Value)));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AuditLogResponse
            {
                Id = l.Id,
                Action = l.Action,
                AccountId = l.AccountId,
                TenantId = l.TenantId,
                UserId = l.UserId,
                Metadata = l.Metadata,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new TenantPagedResponse<AuditLogResponse>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<TenantPagedResponse<AuditLogResponse>> GetTenantLogsAsync(
        int tenantId,
        string? action,
        int? userId,
        string? metadata,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize)
    {
        var query = BuildQuery(action, tenantId, null, userId, metadata, from, to);
        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new AuditLogResponse
            {
                Id = l.Id,
                Action = l.Action,
                AccountId = l.AccountId,
                TenantId = l.TenantId,
                UserId = l.UserId,
                Metadata = l.Metadata,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return new TenantPagedResponse<AuditLogResponse>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    private IQueryable<Models.AuditLog> BuildQuery(
        string? action,
        int? tenantId,
        int? accountId,
        int? userId,
        string? metadata,
        DateTime? from,
        DateTime? to)
    {
        var query = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(action))
        {
            query = query.Where(l => l.Action.Contains(action));
        }

        if (tenantId.HasValue)
        {
            query = query.Where(l => l.TenantId == tenantId);
        }

        if (accountId.HasValue)
        {
            query = query.Where(l => l.AccountId == accountId);
        }

        if (userId.HasValue)
        {
            query = query.Where(l => l.UserId == userId);
        }

        if (!string.IsNullOrWhiteSpace(metadata))
        {
            query = query.Where(l => l.Metadata != null && l.Metadata.Contains(metadata));
        }

        if (from.HasValue)
        {
            query = query.Where(l => l.CreatedAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(l => l.CreatedAt <= to.Value);
        }

        return query;
    }
}
