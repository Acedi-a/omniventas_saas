namespace SaaSEventos.DTOs.Orders;

public class CreateOrderRequest
{
    public List<CreateOrderItemRequest> Items { get; set; } = new();
    public string? CouponCode { get; set; }
}
