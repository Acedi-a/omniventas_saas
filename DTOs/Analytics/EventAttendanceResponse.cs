namespace SaaSEventos.DTOs.Analytics;

public class EventAttendanceResponse
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public int TotalTickets { get; set; }
    public int RedeemedTickets { get; set; }
}
