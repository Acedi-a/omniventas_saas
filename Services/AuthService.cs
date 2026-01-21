using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.Auth;
using SaaSEventos.Helpers;
using SaaSEventos.Models;
using SaaSEventos.Models.Enums;

namespace SaaSEventos.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly JwtHelper _jwtHelper;

    public AuthService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;

        var jwtSection = configuration.GetSection("Jwt");
        var secret = jwtSection.GetValue<string>("Secret") ?? string.Empty;
        var issuer = jwtSection.GetValue<string>("Issuer") ?? string.Empty;
        var audience = jwtSection.GetValue<string>("Audience") ?? string.Empty;
        var expirationHours = jwtSection.GetValue<int>("ExpirationHours");

        _jwtHelper = new JwtHelper(secret, issuer, audience, expirationHours);
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.ApiKey == request.ApiKey);
        if (tenant == null)
        {
            throw new InvalidOperationException("Invalid tenant API key.");
        }

        var emailExists = await _db.Users.AnyAsync(u => u.TenantId == tenant.Id && u.Email == request.Email);
        if (emailExists)
        {
            throw new InvalidOperationException("Email already registered.");
        }

        var user = new User
        {
            TenantId = tenant.Id,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return new RegisterResponse
        {
            UserId = user.Id,
            TenantId = user.TenantId,
            Email = user.Email
        };
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var tenant = await _db.Tenants.FirstOrDefaultAsync(t => t.ApiKey == request.ApiKey);
        if (tenant == null)
        {
            throw new UnauthorizedAccessException("Invalid tenant API key.");
        }

        var user = await _db.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.TenantId == tenant.Id && u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var token = _jwtHelper.GenerateToken(user);

        return new LoginResponse
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt
        };
    }
}
