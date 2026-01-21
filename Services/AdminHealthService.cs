using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.Admin;
using SaaSEventos.Models.Enums;

namespace SaaSEventos.Services;

public class AdminHealthService
{
    private static readonly DateTime StartedAt = DateTime.UtcNow;
    private readonly AppDbContext _db;

    public AdminHealthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<AdminHealthSummaryResponse> GetSummaryAsync()
    {
        var now = DateTime.UtcNow;
        var last24h = now.AddHours(-24);
        var last7d = now.AddDays(-7);
        var last30d = now.AddDays(-30);

        var databaseConnected = await _db.Database.CanConnectAsync();
        var totalTenants = await _db.Tenants.CountAsync();
        var activeTenants = await _db.Tenants.CountAsync(t => t.IsActive);
        var activeTenantsLast30Days = await _db.Orders
            .Where(o => o.CreatedAt >= last30d)
            .Select(o => o.TenantId)
            .Distinct()
            .CountAsync();

        var paidOrdersLast24h = await _db.Orders
            .CountAsync(o => o.Status == OrderStatus.Paid && o.PaidAt.HasValue && o.PaidAt >= last24h);
        var paidOrdersLast7d = await _db.Orders
            .CountAsync(o => o.Status == OrderStatus.Paid && o.PaidAt.HasValue && o.PaidAt >= last7d);
        var revenueLast24h = await _db.Orders
            .Where(o => o.Status == OrderStatus.Paid && o.PaidAt.HasValue && o.PaidAt >= last24h)
            .SumAsync(o => (decimal?)o.Total) ?? 0m;
        var revenueLast7d = await _db.Orders
            .Where(o => o.Status == OrderStatus.Paid && o.PaidAt.HasValue && o.PaidAt >= last7d)
            .SumAsync(o => (decimal?)o.Total) ?? 0m;

        var pendingOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
        var cancelledOrders = await _db.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled);
        var lastOrderAt = await _db.Orders.MaxAsync(o => (DateTime?)o.CreatedAt);
        var lastTenantAt = await _db.Tenants.MaxAsync(t => (DateTime?)t.CreatedAt);
        var lastUserAt = await _db.Users.MaxAsync(u => (DateTime?)u.CreatedAt);

        return new AdminHealthSummaryResponse
        {
            StartedAt = StartedAt,
            UptimeHours = (now - StartedAt).TotalHours,
            DatabaseConnected = databaseConnected,
            TotalTenants = totalTenants,
            ActiveTenants = activeTenants,
            ActiveTenantsLast30Days = activeTenantsLast30Days,
            PaidOrdersLast24Hours = paidOrdersLast24h,
            PaidOrdersLast7Days = paidOrdersLast7d,
            RevenueLast24Hours = revenueLast24h,
            RevenueLast7Days = revenueLast7d,
            PendingOrders = pendingOrders,
            CancelledOrders = cancelledOrders,
            LastOrderAt = lastOrderAt,
            LastTenantAt = lastTenantAt,
            LastUserAt = lastUserAt
        };
    }

    public async Task<List<AdminHealthTrendPoint>> GetTrendsAsync(int days)
    {
        if (days < 7)
        {
            days = 7;
        }

        var startDate = DateTime.UtcNow.Date.AddDays(-days + 1);
        var paidOrders = await _db.Orders
            .Where(o => o.Status == OrderStatus.Paid && o.PaidAt.HasValue && o.PaidAt >= startDate)
            .GroupBy(o => o.PaidAt!.Value.Date)
            .Select(g => new
            {
                Date = g.Key,
                Orders = g.Count(),
                Revenue = g.Sum(o => o.Total)
            })
            .ToListAsync();

        var map = paidOrders.ToDictionary(x => x.Date, x => x);
        var result = new List<AdminHealthTrendPoint>();

        for (var i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            map.TryGetValue(date, out var point);
            result.Add(new AdminHealthTrendPoint
            {
                Date = date.ToString("yyyy-MM-dd"),
                PaidOrders = point?.Orders ?? 0,
                Revenue = point?.Revenue ?? 0m
            });
        }

        return result;
    }
}
