using SaaSEventos.Models.Enums;

namespace SaaSEventos.DTOs.Client;

public class ClientRegisterRequest
{
    public string TenantName { get; set; } = string.Empty;
    public BusinessType BusinessType { get; set; }
    public string AdminEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
