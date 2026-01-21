using SaaSEventos.Models.Enums;

namespace SaaSEventos.DTOs.Orders;

public class OrderSummaryResponse
{
    public int Id { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
}
