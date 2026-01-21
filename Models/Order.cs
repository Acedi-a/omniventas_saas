using SaaSEventos.Models.Enums;

namespace SaaSEventos.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int TenantId { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public string? PaymentQRUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }

    public User User { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
    public List<OrderItem> Items { get; set; } = new();
    public List<Ticket> Tickets { get; set; } = new();
}
