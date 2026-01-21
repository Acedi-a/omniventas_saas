namespace SaaSEventos.DTOs.Events;

public class CreateEventRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
    public decimal Price { get; set; }
}
