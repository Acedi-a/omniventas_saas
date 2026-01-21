namespace SaaSEventos.DTOs.Admin;

public class TenantSummaryResponse
{
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int InactiveTenants { get; set; }
    public int CommerceTenants { get; set; }
    public int EventsTenants { get; set; }
    public int HybridTenants { get; set; }
    public int NewLast7Days { get; set; }
    public int NewLast30Days { get; set; }
    public int ActiveLast30Days { get; set; }
    public int PaidOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal TotalSales { get; set; }
}
