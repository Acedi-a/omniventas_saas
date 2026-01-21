using SaaSEventos.Models.Enums;

namespace SaaSEventos.DTOs.Orders;

public class TenantOrderSummaryResponse
{
    public int Id { get; set; }
    public string BuyerEmail { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
