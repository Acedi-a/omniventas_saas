using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.TenantAuth;
using SaaSEventos.Helpers;
using SaaSEventos.Models.Enums;

namespace SaaSEventos.Services;

public class TenantAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtHelper _jwtHelper;
    private readonly AuditService _auditService;

    public TenantAuthService(AppDbContext db, IConfiguration configuration, AuditService auditService)
    {
        _db = db;
        _auditService = auditService;

        var jwtSection = configuration.GetSection("Jwt");
        var secret = jwtSection.GetValue<string>("Secret") ?? string.Empty;
        var issuer = jwtSection.GetValue<string>("Issuer") ?? string.Empty;
        var audience = jwtSection.GetValue<string>("Audience") ?? string.Empty;
        var expirationHours = jwtSection.GetValue<int>("ExpirationHours");

        _jwtHelper = new JwtHelper(secret, issuer, audience, expirationHours);
    }

    public async Task<TenantLoginResponse> LoginAsync(TenantLoginRequest request)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.Slug == request.TenantSlug);
        if (tenant == null)
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var user = await _db.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        if (user.Role != UserRole.Admin && user.Role != UserRole.Validator)
        {
            throw new UnauthorizedAccessException("User is not allowed.");
        }

        if (!user.Tenant.IsActive)
        {
            throw new UnauthorizedAccessException("Tenant is inactive.");
        }

        var token = _jwtHelper.GenerateToken(user);

        await _auditService.LogAsync("tenant.login", tenantId: user.TenantId, userId: user.Id);

        return new TenantLoginResponse
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            TenantId = user.TenantId,
            TenantName = user.Tenant.Name,
            ApiKey = user.Tenant.ApiKey,
            TenantSlug = user.Tenant.Slug,
            Role = user.Role
        };
    }
}
