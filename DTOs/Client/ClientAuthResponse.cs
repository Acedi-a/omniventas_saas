using SaaSEventos.Models.Enums;

namespace SaaSEventos.DTOs.Client;

public class ClientAuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public BusinessType BusinessType { get; set; }
}
