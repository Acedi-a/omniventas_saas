using SaaSEventos.Models.Enums;

namespace SaaSEventos.DTOs.Orders;

public class OrderDetailResponse
{
    public int Id { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public string? PaymentQRUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
}
