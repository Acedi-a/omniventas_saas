namespace SaaSEventos.DTOs.Auth;

public class RegisterRequest
{
    public string ApiKey { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
