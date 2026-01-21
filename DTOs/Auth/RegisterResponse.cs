namespace SaaSEventos.DTOs.Auth;

public class RegisterResponse
{
    public int UserId { get; set; }
    public int TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
}
