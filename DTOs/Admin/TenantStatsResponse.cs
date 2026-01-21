namespace SaaSEventos.DTOs.Admin;

public class TenantStatsResponse
{
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UsersCount { get; set; }
    public int ProductsCount { get; set; }
    public int EventsCount { get; set; }
    public int OrdersCount { get; set; }
    public decimal TotalSales { get; set; }
}
