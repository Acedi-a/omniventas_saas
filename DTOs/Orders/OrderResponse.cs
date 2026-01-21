namespace SaaSEventos.DTOs.Orders;

public class OrderResponse
{
    public int OrderId { get; set; }
    public decimal Total { get; set; }
    public string PaymentQRUrl { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}
