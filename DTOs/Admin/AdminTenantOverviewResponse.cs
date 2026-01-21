using SaaSEventos.Models.Enums;

namespace SaaSEventos.DTOs.Admin;

public class AdminTenantOverviewResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public BusinessType BusinessType { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int UsersCount { get; set; }
    public int OrdersCount { get; set; }
    public int PaidOrdersCount { get; set; }
    public int OrdersLast30Days { get; set; }
    public decimal TotalSales { get; set; }
    public DateTime? LastOrderAt { get; set; }
    public DateTime? LastUserAt { get; set; }
    public DateTime? LastEventAt { get; set; }
    public DateTime? LastActivityAt { get; set; }
}
