using SaaSEventos.Models.Enums;

namespace SaaSEventos.DTOs.Owner;

public class OwnerTenantResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public BusinessType BusinessType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
