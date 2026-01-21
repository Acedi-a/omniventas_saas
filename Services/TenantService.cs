using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.Admin;
using SaaSEventos.DTOs.Tenants;
using SaaSEventos.Models.Enums;
using SaaSEventos.Models;

namespace SaaSEventos.Services;

public class TenantService
{
    private readonly AppDbContext _db;

    public TenantService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TenantResponse> CreateTenantAsync(CreateTenantRequest request)
    {
        var tenant = new Tenant
        {
            Name = request.Name,
            BusinessType = request.BusinessType,
            ApiKey = $"tn_live_{Guid.NewGuid():N}",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();

        return new TenantResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            ApiKey = tenant.ApiKey,
            BusinessType = tenant.BusinessType,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt
        };
    }

    public async Task<TenantResponse?> GetTenantAsync(int id)
    {
        return await _db.Tenants
            .Where(t => t.Id == id)
            .Select(t => new TenantResponse
            {
                Id = t.Id,
                Name = t.Name,
                ApiKey = t.ApiKey,
                BusinessType = t.BusinessType,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<TenantResponse>> GetTenantsAsync()
    {
        return await _db.Tenants
            .OrderBy(t => t.CreatedAt)
            .Select(t => new TenantResponse
            {
                Id = t.Id,
                Name = t.Name,
                ApiKey = t.ApiKey,
                BusinessType = t.BusinessType,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<TenantPagedResponse<TenantResponse>> GetTenantsPagedAsync(
        string? search,
        BusinessType? businessType,
        bool? isActive,
        int page,
        int pageSize)
    {
        var query = _db.Tenants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => t.Name.ToLower().Contains(search.ToLower()));
        }

        if (businessType.HasValue)
        {
            query = query.Where(t => t.BusinessType == businessType.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(t => t.IsActive == isActive.Value);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TenantResponse
            {
                Id = t.Id,
                Name = t.Name,
                ApiKey = t.ApiKey,
                BusinessType = t.BusinessType,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();

        return new TenantPagedResponse<TenantResponse>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<TenantSummaryResponse> GetSummaryAsync()
    {
        var total = await _db.Tenants.CountAsync();
        var active = await _db.Tenants.CountAsync(t => t.IsActive);
        var inactive = total - active;
        var commerce = await _db.Tenants.CountAsync(t => t.BusinessType == BusinessType.Commerce);
        var events = await _db.Tenants.CountAsync(t => t.BusinessType == BusinessType.Events);
        var hybrid = await _db.Tenants.CountAsync(t => t.BusinessType == BusinessType.Hybrid);

        var now = DateTime.UtcNow;
        var last7 = await _db.Tenants.CountAsync(t => t.CreatedAt >= now.AddDays(-7));
        var last30 = await _db.Tenants.CountAsync(t => t.CreatedAt >= now.AddDays(-30));

        return new TenantSummaryResponse
        {
            TotalTenants = total,
            ActiveTenants = active,
            InactiveTenants = inactive,
            CommerceTenants = commerce,
            EventsTenants = events,
            HybridTenants = hybrid,
            NewLast7Days = last7,
            NewLast30Days = last30
        };
    }

    public async Task<List<TenantTrendPoint>> GetTenantTrendsAsync(int days)
    {
        if (days < 1)
        {
            days = 30;
        }

        var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);
        var grouped = await _db.Tenants
            .Where(t => t.CreatedAt >= startDate)
            .GroupBy(t => t.CreatedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var map = grouped.ToDictionary(x => x.Date, x => x.Count);
        var result = new List<TenantTrendPoint>();

        for (var i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            map.TryGetValue(date, out var count);
            result.Add(new TenantTrendPoint
            {
                Date = date.ToString("yyyy-MM-dd"),
                Count = count
            });
        }

        return result;
    }

    public async Task<List<RecentTenantResponse>> GetRecentTenantsAsync(int limit)
    {
        if (limit < 1)
        {
            limit = 5;
        }

        return await _db.Tenants
            .OrderByDescending(t => t.CreatedAt)
            .Take(limit)
            .Select(t => new RecentTenantResponse
            {
                Id = t.Id,
                Name = t.Name,
                BusinessType = t.BusinessType,
                IsActive = t.IsActive,
                CreatedAt = t.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<TenantStatsResponse?> GetTenantStatsAsync(int tenantId)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
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

    public async Task<TenantResponse?> UpdateTenantStatusAsync(int tenantId, bool isActive)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
        if (tenant == null)
        {
            return null;
        }

        tenant.IsActive = isActive;
        await _db.SaveChangesAsync();

        return new TenantResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            ApiKey = tenant.ApiKey,
            BusinessType = tenant.BusinessType,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt
        };
    }

    public async Task<TenantResponse?> RegenerateApiKeyAsync(int tenantId)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId);
        if (tenant == null)
        {
            return null;
        }

        tenant.ApiKey = $"tn_live_{Guid.NewGuid():N}";
        await _db.SaveChangesAsync();

        return new TenantResponse
        {
            Id = tenant.Id,
            Name = tenant.Name,
            ApiKey = tenant.ApiKey,
            BusinessType = tenant.BusinessType,
            IsActive = tenant.IsActive,
            CreatedAt = tenant.CreatedAt
        };
    }
}
