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

    public async Task<TenantPagedResponse<AdminTenantOverviewResponse>> GetTenantsPagedAsync(
        string? search,
        BusinessType? businessType,
        bool? isActive,
        DateTime? createdFrom,
        DateTime? createdTo,
        decimal? minSales,
        decimal? maxSales,
        int? minOrders,
        int? maxOrders,
        int? activityDays,
        int page,
        int pageSize)
    {
        var now = DateTime.UtcNow;
        var query = BuildAdminTenantQuery(
            search,
            businessType,
            isActive,
            createdFrom,
            createdTo,
            minSales,
            maxSales,
            minOrders,
            maxOrders,
            activityDays,
            now);

        var total = await query.CountAsync();
        var items = await ProjectAdminTenant(query, now.AddDays(-30))
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        AddLastActivity(items);

        return new TenantPagedResponse<AdminTenantOverviewResponse>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<List<AdminTenantOverviewResponse>> GetTenantsExportAsync(
        string? search,
        BusinessType? businessType,
        bool? isActive,
        DateTime? createdFrom,
        DateTime? createdTo,
        decimal? minSales,
        decimal? maxSales,
        int? minOrders,
        int? maxOrders,
        int? activityDays)
    {
        var now = DateTime.UtcNow;
        var query = BuildAdminTenantQuery(
            search,
            businessType,
            isActive,
            createdFrom,
            createdTo,
            minSales,
            maxSales,
            minOrders,
            maxOrders,
            activityDays,
            now);

        var items = await ProjectAdminTenant(query, now.AddDays(-30))
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        AddLastActivity(items);
        return items;
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
        var activeLast30 = await _db.Orders
            .Where(o => o.CreatedAt >= now.AddDays(-30))
            .Select(o => o.TenantId)
            .Distinct()
            .CountAsync();
        var paidOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Paid);
        var pendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
        var cancelledOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled);
        var totalSales = await _db.Orders
            .Where(o => o.Status == OrderStatus.Paid)
            .SumAsync(o => (decimal?)o.Total) ?? 0m;

        return new TenantSummaryResponse
        {
            TotalTenants = total,
            ActiveTenants = active,
            InactiveTenants = inactive,
            CommerceTenants = commerce,
            EventsTenants = events,
            HybridTenants = hybrid,
            NewLast7Days = last7,
            NewLast30Days = last30,
            ActiveLast30Days = activeLast30,
            PaidOrders = paidOrders,
            PendingOrders = pendingOrders,
            CancelledOrders = cancelledOrders,
            TotalSales = totalSales
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
        var orders = _db.Orders.Where(o => o.TenantId == tenantId);
        var ordersCount = await orders.CountAsync();
        var paidOrdersCount = await orders.CountAsync(o => o.Status == OrderStatus.Paid);
        var pendingOrdersCount = await orders.CountAsync(o => o.Status == OrderStatus.Pending);
        var cancelledOrdersCount = await orders.CountAsync(o => o.Status == OrderStatus.Cancelled);
        var totalSales = await orders
            .Where(o => o.Status == OrderStatus.Paid)
            .SumAsync(o => (decimal?)o.Total) ?? 0m;
        var lastOrder = await orders
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new { o.CreatedAt, o.Total })
            .FirstOrDefaultAsync();
        var lastUserAt = await _db.Users
            .Where(u => u.TenantId == tenantId)
            .MaxAsync(u => (DateTime?)u.CreatedAt);
        var lastEventAt = await _db.Events
            .Where(e => e.TenantId == tenantId)
            .MaxAsync(e => (DateTime?)e.CreatedAt);
        var now = DateTime.UtcNow;
        var activeCoupons = await _db.Coupons
            .CountAsync(c => c.TenantId == tenantId && c.ExpiresAt > now && c.CurrentUses < c.MaxUses);

        return new TenantStatsResponse
        {
            TenantId = tenant.Id,
            Name = tenant.Name,
            UsersCount = users,
            ProductsCount = products,
            EventsCount = events,
            OrdersCount = ordersCount,
            PaidOrdersCount = paidOrdersCount,
            PendingOrdersCount = pendingOrdersCount,
            CancelledOrdersCount = cancelledOrdersCount,
            ActiveCouponsCount = activeCoupons,
            TotalSales = totalSales,
            LastOrderAt = lastOrder?.CreatedAt,
            LastOrderTotal = lastOrder?.Total,
            LastUserAt = lastUserAt,
            LastEventAt = lastEventAt
        };
    }

    public async Task<List<TenantActivityItem>> GetTenantActivityAsync(int tenantId, int limit)
    {
        if (limit < 1)
        {
            limit = 10;
        }

        var orders = await _db.Orders
            .Where(o => o.TenantId == tenantId)
            .OrderByDescending(o => o.CreatedAt)
            .Take(limit)
            .Select(o => new TenantActivityItem
            {
                Type = "order",
                Title = $"Orden #{o.Id}",
                Description = o.Status.ToString(),
                OccurredAt = o.CreatedAt,
                Amount = o.Total,
                ReferenceId = o.Id
            })
            .ToListAsync();

        var users = await _db.Users
            .Where(u => u.TenantId == tenantId)
            .OrderByDescending(u => u.CreatedAt)
            .Take(limit)
            .Select(u => new TenantActivityItem
            {
                Type = "user",
                Title = "Nuevo usuario",
                Description = u.Email,
                OccurredAt = u.CreatedAt,
                ReferenceId = u.Id
            })
            .ToListAsync();

        var products = await _db.Products
            .Where(p => p.TenantId == tenantId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(limit)
            .Select(p => new TenantActivityItem
            {
                Type = "product",
                Title = "Nuevo producto",
                Description = p.Name,
                OccurredAt = p.CreatedAt,
                Amount = p.Price,
                ReferenceId = p.Id
            })
            .ToListAsync();

        var eventsList = await _db.Events
            .Where(e => e.TenantId == tenantId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(limit)
            .Select(e => new TenantActivityItem
            {
                Type = "event",
                Title = "Nuevo evento",
                Description = e.Name,
                OccurredAt = e.CreatedAt,
                Amount = e.Price,
                ReferenceId = e.Id
            })
            .ToListAsync();

        var coupons = await _db.Coupons
            .Where(c => c.TenantId == tenantId)
            .OrderByDescending(c => c.ExpiresAt)
            .Take(limit)
            .Select(c => new TenantActivityItem
            {
                Type = "coupon",
                Title = "Cupon creado",
                Description = c.Code,
                OccurredAt = c.ExpiresAt,
                ReferenceId = c.Id
            })
            .ToListAsync();

        return orders
            .Concat(users)
            .Concat(products)
            .Concat(eventsList)
            .Concat(coupons)
            .OrderByDescending(item => item.OccurredAt)
            .Take(limit)
            .ToList();
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

    private IQueryable<Tenant> BuildAdminTenantQuery(
        string? search,
        BusinessType? businessType,
        bool? isActive,
        DateTime? createdFrom,
        DateTime? createdTo,
        decimal? minSales,
        decimal? maxSales,
        int? minOrders,
        int? maxOrders,
        int? activityDays,
        DateTime now)
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

        if (createdFrom.HasValue)
        {
            var startDate = createdFrom.Value.Date;
            query = query.Where(t => t.CreatedAt >= startDate);
        }

        if (createdTo.HasValue)
        {
            var endDate = createdTo.Value.Date.AddDays(1);
            query = query.Where(t => t.CreatedAt < endDate);
        }

        if (minOrders.HasValue && minOrders.Value >= 0)
        {
            query = query.Where(t => t.Orders.Count() >= minOrders.Value);
        }

        if (maxOrders.HasValue && maxOrders.Value >= 0)
        {
            query = query.Where(t => t.Orders.Count() <= maxOrders.Value);
        }

        if (activityDays.HasValue && activityDays.Value > 0)
        {
            var cutoff = now.AddDays(-activityDays.Value);
            query = query.Where(t => t.Orders.Any(o => o.CreatedAt >= cutoff));
        }

        if (minSales.HasValue && minSales.Value >= 0)
        {
            query = query.Where(t =>
                (t.Orders.Where(o => o.Status == OrderStatus.Paid).Sum(o => (decimal?)o.Total) ?? 0m) >=
                minSales.Value);
        }

        if (maxSales.HasValue && maxSales.Value >= 0)
        {
            query = query.Where(t =>
                (t.Orders.Where(o => o.Status == OrderStatus.Paid).Sum(o => (decimal?)o.Total) ?? 0m) <=
                maxSales.Value);
        }

        return query;
    }

    private static IQueryable<AdminTenantOverviewResponse> ProjectAdminTenant(
        IQueryable<Tenant> query,
        DateTime last30Days)
    {
        return query.Select(t => new AdminTenantOverviewResponse
        {
            Id = t.Id,
            Name = t.Name,
            ApiKey = t.ApiKey,
            BusinessType = t.BusinessType,
            IsActive = t.IsActive,
            CreatedAt = t.CreatedAt,
            UsersCount = t.Users.Count(),
            OrdersCount = t.Orders.Count(),
            PaidOrdersCount = t.Orders.Count(o => o.Status == OrderStatus.Paid),
            OrdersLast30Days = t.Orders.Count(o => o.CreatedAt >= last30Days),
            TotalSales = t.Orders.Where(o => o.Status == OrderStatus.Paid)
                .Sum(o => (decimal?)o.Total) ?? 0m,
            LastOrderAt = t.Orders.Max(o => (DateTime?)o.CreatedAt),
            LastUserAt = t.Users.Max(u => (DateTime?)u.CreatedAt),
            LastEventAt = t.Events.Max(e => (DateTime?)e.CreatedAt)
        });
    }

    private static void AddLastActivity(IEnumerable<AdminTenantOverviewResponse> items)
    {
        foreach (var item in items)
        {
            var activityDates = new List<DateTime?> { item.LastOrderAt, item.LastUserAt, item.LastEventAt }
                .Where(date => date.HasValue)
                .Select(date => date!.Value)
                .ToList();

            item.LastActivityAt = activityDates.Count == 0 ? null : activityDates.Max();
        }
    }
}
