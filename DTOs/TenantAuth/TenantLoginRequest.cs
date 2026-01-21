namespace SaaSEventos.DTOs.TenantAuth;

public class TenantLoginRequest
{
    public string TenantSlug { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
