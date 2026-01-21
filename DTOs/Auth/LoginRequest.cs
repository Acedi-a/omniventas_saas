namespace SaaSEventos.DTOs.Auth;

public class LoginRequest
{
    public string ApiKey { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
