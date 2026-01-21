using SaaSEventos.Models.Enums;

namespace SaaSEventos.Models;

public class User
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public List<Order> Orders { get; set; } = new();
    public List<Ticket> Tickets { get; set; } = new();
}
