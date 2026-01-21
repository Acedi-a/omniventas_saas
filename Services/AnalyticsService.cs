using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.Analytics;
using SaaSEventos.Models.Enums;

namespace SaaSEventos.Services;

public class AnalyticsService
{
    private readonly AppDbContext _db;

    public AnalyticsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SalesStatsResponse> GetSalesStatsAsync(int tenantId, string? period)
    {
        DateTime? startDate = period?.ToLowerInvariant() switch
        {
            "week" => DateTime.UtcNow.AddDays(-7),
            "month" => DateTime.UtcNow.AddMonths(-1),
            "year" => DateTime.UtcNow.AddYears(-1),
            _ => null
        };

        var query = _db.Orders
            .Where(o => o.TenantId == tenantId && o.Status == OrderStatus.Paid);

        if (startDate.HasValue)
        {
            query = query.Where(o => o.PaidAt >= startDate);
        }

        var totalSales = await query.SumAsync(o => (decimal?)o.Total) ?? 0m;
        var orderCount = await query.CountAsync();
        var average = orderCount == 0 ? 0m : totalSales / orderCount;

        return new SalesStatsResponse
        {
            TotalSales = totalSales,
            OrderCount = orderCount,
            AverageOrderValue = average
        };
    }

    public async Task<List<TopProductResponse>> GetTopProductsAsync(int tenantId, int limit)
    {
        return await _db.OrderItems
            .Where(i => i.ProductId != null && i.Order.TenantId == tenantId && i.Order.Status == OrderStatus.Paid)
            .GroupBy(i => new { i.ProductId, i.Product!.Name })
            .Select(group => new TopProductResponse
            {
                ProductId = group.Key.ProductId ?? 0,
                Name = group.Key.Name,
                QuantitySold = group.Sum(i => i.Quantity),
                Revenue = group.Sum(i => i.Subtotal)
            })
            .OrderByDescending(p => p.QuantitySold)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<EventAttendanceResponse?> GetEventAttendanceAsync(int tenantId, int eventId)
    {
        var eventEntity = await _db.Events
            .FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == eventId);

        if (eventEntity == null)
        {
            return null;
        }

        var totalTickets = await _db.Tickets.CountAsync(t => t.EventId == eventId);
        var redeemedTickets = await _db.Tickets.CountAsync(t => t.EventId == eventId && t.Status == TicketStatus.Redeemed);

        return new EventAttendanceResponse
        {
            EventId = eventEntity.Id,
            EventName = eventEntity.Name,
            TotalTickets = totalTickets,
            RedeemedTickets = redeemedTickets
        };
    }
}
