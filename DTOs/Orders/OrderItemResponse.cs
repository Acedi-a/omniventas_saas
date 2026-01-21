namespace SaaSEventos.DTOs.Orders;

public class OrderItemResponse
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public int? EventId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
