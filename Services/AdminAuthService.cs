using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.Admin;
using SaaSEventos.DTOs.Auth;
using SaaSEventos.Helpers;
using SaaSEventos.Models.Enums;

namespace SaaSEventos.Services;

public class AdminAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtHelper _jwtHelper;

    public AdminAuthService(AppDbContext db, IConfiguration configuration)
    {
        _db = db;

        var jwtSection = configuration.GetSection("Jwt");
        var secret = jwtSection.GetValue<string>("Secret") ?? string.Empty;
        var issuer = jwtSection.GetValue<string>("Issuer") ?? string.Empty;
        var audience = jwtSection.GetValue<string>("Audience") ?? string.Empty;
        var expirationHours = jwtSection.GetValue<int>("ExpirationHours");

        _jwtHelper = new JwtHelper(secret, issuer, audience, expirationHours);
    }

    public async Task<LoginResponse> LoginAsync(AdminLoginRequest request)
    {
        var user = await _db.Users
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Role == UserRole.SuperAdmin);

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
