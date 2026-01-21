namespace SaaSEventos.DTOs.Tickets;

public class ValidateTicketResponse
{
    public bool Valid { get; set; }
    public string? Reason { get; set; }
    public string? EventName { get; set; }
    public string? UserEmail { get; set; }
    public DateTime? RedeemedAt { get; set; }
}
