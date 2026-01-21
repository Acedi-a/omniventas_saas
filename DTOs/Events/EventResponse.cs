namespace SaaSEventos.DTOs.Events;

public class EventResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
    public int AvailableTickets { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }
}
