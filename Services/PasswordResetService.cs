using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.TenantAuth;
using SaaSEventos.Models;
using SaaSEventos.Models.Enums;

namespace SaaSEventos.Services;

public class PasswordResetService
{
    private readonly AppDbContext _db;
    private readonly AuditService _auditService;

    public PasswordResetService(AppDbContext db, AuditService auditService)
    {
        _db = db;
        _auditService = auditService;
    }

    public async Task<PasswordForgotResponse> RequestResetAsync(PasswordForgotRequest request)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Slug == request.TenantSlug);
        if (tenant == null)
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        var user = await _db.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.Email == request.Email);

        if (user == null || (user.Role != UserRole.Admin && user.Role != UserRole.Validator))
        {
            throw new UnauthorizedAccessException("User not found.");
        }

        var token = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddHours(1);

        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            Token = token,
            ExpiresAt = expiresAt,
            CreatedAt = DateTime.UtcNow
        };

        _db.PasswordResetTokens.Add(resetToken);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync("tenant.password.requested", tenantId: user.TenantId, userId: user.Id);

        return new PasswordForgotResponse
        {
            Token = token,
            ExpiresAt = expiresAt
        };
    }

    public async Task ResetAsync(PasswordResetRequest request)
    {
        var token = await _db.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == request.Token);

        if (token == null || token.UsedAt.HasValue || token.ExpiresAt <= DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Invalid or expired token.");
        }

        token.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        token.UsedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _auditService.LogAsync("tenant.password.reset", tenantId: token.User.TenantId, userId: token.UserId);
    }
}
