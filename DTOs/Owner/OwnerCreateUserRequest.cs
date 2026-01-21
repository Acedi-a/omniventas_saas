using SaaSEventos.Models.Enums;

namespace SaaSEventos.DTOs.Owner;

public class OwnerCreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
