namespace SaaSEventos.DTOs.TenantAuth;

public class PasswordForgotRequest
{
    public string TenantSlug { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
