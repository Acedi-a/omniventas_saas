using SaaSEventos.Models.Enums;

namespace SaaSEventos.DTOs.Tickets;

public class TicketDetailResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string QRCodeUrl { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
}
