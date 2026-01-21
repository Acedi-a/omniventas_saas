namespace SaaSEventos.DTOs.Orders;

public class ConfirmPaymentTicketResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string QRCodeUrl { get; set; } = string.Empty;
}

public class ConfirmPaymentResponse
{
    public bool Success { get; set; }
    public List<ConfirmPaymentTicketResponse> Tickets { get; set; } = new();
}
