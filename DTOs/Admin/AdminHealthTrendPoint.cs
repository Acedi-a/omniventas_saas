namespace SaaSEventos.DTOs.Admin;

public class AdminHealthTrendPoint
{
    public string Date { get; set; } = string.Empty;
    public int PaidOrders { get; set; }
    public decimal Revenue { get; set; }
}
