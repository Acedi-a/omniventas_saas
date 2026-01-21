namespace SaaSEventos.DTOs.TenantAuth;

public class PasswordResetRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
