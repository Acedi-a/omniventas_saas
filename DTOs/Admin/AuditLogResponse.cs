namespace SaaSEventos.DTOs.Admin;

public class AuditLogResponse
{
    public int Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public int? AccountId { get; set; }
    public int? TenantId { get; set; }
    public int? UserId { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
}
