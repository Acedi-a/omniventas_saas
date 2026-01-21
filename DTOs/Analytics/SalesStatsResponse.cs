namespace SaaSEventos.DTOs.Analytics;

public class SalesStatsResponse
{
    public decimal TotalSales { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
}
