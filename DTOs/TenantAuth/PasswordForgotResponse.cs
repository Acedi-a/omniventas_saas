namespace SaaSEventos.DTOs.TenantAuth;

public class PasswordForgotResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
