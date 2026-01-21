using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.Client;
using SaaSEventos.Helpers;
using SaaSEventos.Models;
using SaaSEventos.Models.Enums;

namespace SaaSEventos.Services;

public class ClientAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtHelper _jwtHelper;

    public ClientAuthService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;

        var jwtSection = configuration.GetSection("Jwt");
        var secret = jwtSection.GetValue<string>("Secret") ?? string.Empty;
        var issuer = jwtSection.GetValue<string>("Issuer") ?? string.Empty;
        var audience = jwtSection.GetValue<string>("Audience") ?? string.Empty;
        var expirationHours = jwtSection.GetValue<int>("ExpirationHours");

        _jwtHelper = new JwtHelper(secret, issuer, audience, expirationHours);
    }

    public async Task<ClientAuthResponse> RegisterAsync(ClientRegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.TenantName))
        {
            throw new InvalidOperationException("Tenant name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.AdminEmail) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new InvalidOperationException("Admin credentials are required.");
        }

        var tenantExists = await _db.Tenants.AnyAsync(t => t.Name.ToLower() == request.TenantName.ToLower());
        if (tenantExists)
        {
            throw new InvalidOperationException("Tenant name already exists.");
        }

        var now = DateTime.UtcNow;
        await using var transaction = await _db.Database.BeginTransactionAsync();

        var tenant = new Tenant
        {
            Name = request.TenantName.Trim(),
            BusinessType = request.BusinessType,
            ApiKey = $"tn_live_{Guid.NewGuid():N}",
            IsActive = true,
            CreatedAt = now
        };

        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();

        var adminUser = new User
        {
            TenantId = tenant.Id,
            Email = request.AdminEmail.Trim().ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Admin,
            CreatedAt = now
        };

        _db.Users.Add(adminUser);
        await _db.SaveChangesAsync();

        await transaction.CommitAsync();

        adminUser.Tenant = tenant;
        var token = _jwtHelper.GenerateToken(adminUser);

        return new ClientAuthResponse
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            ApiKey = tenant.ApiKey,
            BusinessType = tenant.BusinessType
        };
    }

    public async Task<ClientAuthResponse> LoginAsync(ClientLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ApiKey))
        {
            throw new UnauthorizedAccessException("API key is required.");
        }

        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.ApiKey == request.ApiKey);
        if (tenant == null)
        {
            throw new UnauthorizedAccessException("Invalid tenant API key.");
        }

        if (!tenant.IsActive)
        {
            throw new UnauthorizedAccessException("Tenant is inactive.");
        }

        var email = request.Email.Trim().ToLowerInvariant();
        var adminUser = await _db.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.Email == email && u.Role == UserRole.Admin);

        if (adminUser == null || !BCrypt.Net.BCrypt.Verify(request.Password, adminUser.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var token = _jwtHelper.GenerateToken(adminUser);

        return new ClientAuthResponse
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            TenantId = tenant.Id,
            TenantName = tenant.Name,
            ApiKey = tenant.ApiKey,
            BusinessType = tenant.BusinessType
        };
    }
}
