using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SaaSEventos.Data;
using SaaSEventos.DTOs.Auth;
using SaaSEventos.DTOs.Owner;
using SaaSEventos.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SaaSEventos.Services;

public class OwnerAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly AuditService _auditService;

    public OwnerAuthService(AppDbContext db, IConfiguration configuration, AuditService auditService)
    {
        _db = db;
        _configuration = configuration;
        _auditService = auditService;
    }

    public async Task RegisterAsync(OwnerRegisterRequest request)
    {
        var exists = await _db.Accounts.AnyAsync(a => a.Email == request.Email);
        if (exists)
        {
            throw new InvalidOperationException("Email already registered.");
        }

        var account = new Account
        {
            Name = request.Name,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        _db.Accounts.Add(account);
        await _db.SaveChangesAsync();

        await _auditService.LogAsync("owner.register", accountId: account.Id);
    }

    public async Task<LoginResponse> LoginAsync(OwnerLoginRequest request)
    {
        var account = await _db.Accounts.FirstOrDefaultAsync(a => a.Email == request.Email);
        if (account == null || !BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid credentials.");
        }

        var jwtSection = _configuration.GetSection("Jwt");
        var secret = jwtSection.GetValue<string>("Secret") ?? string.Empty;
        var issuer = jwtSection.GetValue<string>("Issuer") ?? string.Empty;
        var audience = jwtSection.GetValue<string>("Audience") ?? string.Empty;
        var expirationHours = jwtSection.GetValue<int>("ExpirationHours");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, account.Id.ToString()),
            new Claim(ClaimTypes.Email, account.Email),
            new Claim(ClaimTypes.Role, "Owner"),
            new Claim("AccountId", account.Id.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddHours(expirationHours);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        };
    }
}
