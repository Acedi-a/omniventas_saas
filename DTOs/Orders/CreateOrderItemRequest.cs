namespace SaaSEventos.DTOs.Orders;

public class CreateOrderItemRequest
{
    public int? ProductId { get; set; }
    public int? EventId { get; set; }
    public int Quantity { get; set; }
}
