namespace SaaSEventos.DTOs.Admin;

public class AdminHealthSummaryResponse
{
    public DateTime StartedAt { get; set; }
    public double UptimeHours { get; set; }
    public bool DatabaseConnected { get; set; }
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int ActiveTenantsLast30Days { get; set; }
    public int PaidOrdersLast24Hours { get; set; }
    public int PaidOrdersLast7Days { get; set; }
    public decimal RevenueLast24Hours { get; set; }
    public decimal RevenueLast7Days { get; set; }
    public int PendingOrders { get; set; }
    public int CancelledOrders { get; set; }
    public DateTime? LastOrderAt { get; set; }
    public DateTime? LastTenantAt { get; set; }
    public DateTime? LastUserAt { get; set; }
}
