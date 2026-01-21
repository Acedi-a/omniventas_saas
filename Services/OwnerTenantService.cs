using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.Admin;
using SaaSEventos.DTOs.Tenants;
using SaaSEventos.DTOs.Owner;
using SaaSEventos.Models;
using SaaSEventos.Models.Enums;
using SaaSEventos.Helpers;

namespace SaaSEventos.Services;

public class OwnerTenantService
{
    private readonly AppDbContext _db;
    private readonly AuditService _auditService;

    public OwnerTenantService(AppDbContext db, AuditService auditService)
    {
        _db = db;
        _auditService = auditService;
    }

    public async Task<List<OwnerTenantResponse>> GetTenantsAsync(int accountId)
    {
        return await _db.Tenants
            .Where(t => t.AccountId == accountId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new OwnerTenantResponse
            {
                Id = t.Id,
                Name = t.Name,
                Slug = t.Slug,
                ApiKey = t.ApiKey,
                BusinessType = t.BusinessType,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<OwnerTenantResponse> CreateTenantAsync(int accountId, CreateTenantRequest request)
    {
        var slug = await GenerateUniqueSlugAsync(request.Name);
        var tenant = new Tenant
        {
            AccountId = accountId,
            Name = request.Name,
            Slug = slug,
            BusinessType = request.BusinessType,
            ApiKey = $"tn_live_{Guid.NewGuid():N}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync("owner.tenant.created", accountId: accountId, tenantId: tenant.Id);

        return new OwnerTenantResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Slug,
            ApiKey = tenant.ApiKey,
            BusinessType = tenant.BusinessType,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt
        };
    }

    public async Task<TenantStatsResponse?> GetTenantStatsAsync(int accountId, int tenantId)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId && t.AccountId == accountId);
        if (tenant == null)
        {
            return null;
        }

        var users = await _db.Users.CountAsync(u => u.TenantId == tenantId);
        var products = await _db.Products.CountAsync(p => p.TenantId == tenantId);
        var events = await _db.Events.CountAsync(e => e.TenantId == tenantId);
        var orders = await _db.Orders.CountAsync(o => o.TenantId == tenantId);
        var totalSales = await _db.Orders
            .Where(o => o.TenantId == tenantId && o.Status == OrderStatus.Paid)
            .SumAsync(o => (decimal?)o.Total) ?? 0m;

        return new TenantStatsResponse
        {
            TenantId = tenant.Id,
            Name = tenant.Name,
            UsersCount = users,
            ProductsCount = products,
            EventsCount = events,
            OrdersCount = orders,
            TotalSales = totalSales
        };
    }

    public async Task<OwnerTenantResponse?> UpdateSlugAsync(int accountId, int tenantId, string slug)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId && t.AccountId == accountId);
        if (tenant == null)
        {
            return null;
        }

        var normalized = SlugHelper.Generate(slug);
        var exists = await _db.Tenants.AnyAsync(t => t.Slug == normalized && t.Id != tenantId);
        if (exists)
        {
            throw new InvalidOperationException("Slug already in use.");
        }

        tenant.Slug = normalized;
        await _db.SaveChangesAsync();

        await _auditService.LogAsync("owner.tenant.slug_updated", accountId: accountId, tenantId: tenantId, metadata: normalized);

        return new OwnerTenantResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Slug = tenant.Slug,
            ApiKey = tenant.ApiKey,
            BusinessType = tenant.BusinessType,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt
        };
    }

    public async Task<SlugAvailabilityResponse> CheckSlugAvailabilityAsync(int accountId, int tenantId, string slug)
    {
        var normalized = SlugHelper.Generate(slug);
        var exists = await _db.Tenants.AnyAsync(t => t.Slug == normalized && t.Id != tenantId);

        return new SlugAvailabilityResponse
        {
            Available = !exists,
            NormalizedSlug = normalized
        };
    }

    private async Task<string> GenerateUniqueSlugAsync(string name)
    {
        var baseSlug = SlugHelper.Generate(name);
        var slug = baseSlug;
        var counter = 1;

        while (await _db.Tenants.AnyAsync(t => t.Slug == slug))
        {
            counter++;
            slug = $"{baseSlug}-{counter}";
        }

        return slug;
    }
}
