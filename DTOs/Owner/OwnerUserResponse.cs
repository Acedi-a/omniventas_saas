using SaaSEventos.Models.Enums;

namespace SaaSEventos.DTOs.Owner;

public class OwnerUserResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
}
