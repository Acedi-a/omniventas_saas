using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.TenantAuth;

namespace SaaSEventos.Services;

public class TenantOrdersService
{
    private readonly AppDbContext _db;

    public TenantOrdersService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TenantOrderResponse>> GetOrdersAsync(int tenantId)
    {
        return await _db.Orders
            .Where(o => o.TenantId == tenantId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new TenantOrderResponse
            {
                Id = o.Id,
                BuyerEmail = o.User.Email,
                Total = o.Total,
                Status = o.Status,
                CreatedAt = o.CreatedAt,
                PaidAt = o.PaidAt
            })
            .ToListAsync();
    }
}
