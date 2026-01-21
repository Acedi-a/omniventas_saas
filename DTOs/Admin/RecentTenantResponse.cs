using SaaSEventos.Models.Enums;

namespace SaaSEventos.DTOs.Admin;

public class RecentTenantResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public BusinessType BusinessType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
