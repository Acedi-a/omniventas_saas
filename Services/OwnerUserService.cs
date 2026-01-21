using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.Owner;
using SaaSEventos.Models;
using SaaSEventos.Models.Enums;

namespace SaaSEventos.Services;

public class OwnerUserService
{
    private readonly AppDbContext _db;
    private readonly AuditService _auditService;

    public OwnerUserService(AppDbContext db, AuditService auditService)
    {
        _db = db;
        _auditService = auditService;
    }

    public async Task<List<OwnerUserResponse>?> GetUsersAsync(int accountId, int tenantId)
    {
        var ownsTenant = await _db.Tenants.AnyAsync(t => t.Id == tenantId && t.AccountId == accountId);
        if (!ownsTenant)
        {
            return null;
        }

        return await _db.Users
            .Where(u => u.TenantId == tenantId)
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new OwnerUserResponse
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<OwnerUserResponse?> CreateUserAsync(int accountId, int tenantId, OwnerCreateUserRequest request)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId && t.AccountId == accountId);
        if (tenant == null)
        {
            return null;
        }

        if (request.Role != UserRole.Admin && request.Role != UserRole.Validator)
        {
            throw new InvalidOperationException("Role not allowed.");
        }

        var exists = await _db.Users.AnyAsync(u => u.TenantId == tenantId && u.Email == request.Email);
        if (exists)
        {
            throw new InvalidOperationException("Email already registered.");
        }

        var user = new User
        {
            TenantId = tenantId,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync("owner.user.created", tenantId: tenantId, userId: user.Id);

        return new OwnerUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<OwnerUserResponse?> ResetPasswordAsync(int accountId, int tenantId, int userId, string newPassword)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Id == tenantId && t.AccountId == accountId);
        if (tenant == null)
        {
            return null;
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.TenantId == tenantId);
        if (user == null)
        {
            return null;
        }

        if (user.Role != UserRole.Admin && user.Role != UserRole.Validator)
        {
            throw new InvalidOperationException("Role not allowed.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync("owner.user.password_reset", tenantId: tenantId, userId: user.Id);

        return new OwnerUserResponse
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }
}
