using SaaSEventos.Models.Enums;

namespace SaaSEventos.DTOs.Tickets;

public class TicketSummaryResponse
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public TicketStatus Status { get; set; }
    public string QRCodeUrl { get; set; } = string.Empty;
}
