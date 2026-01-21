namespace SaaSEventos.Models;

public class AuditLog
{
    public int Id { get; set; }
    public int? AccountId { get; set; }
    public int? TenantId { get; set; }
    public int? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}
