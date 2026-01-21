using SaaSEventos.Models.Enums;

namespace SaaSEventos.DTOs.TenantAuth;

public class TenantLoginResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string TenantSlug { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
