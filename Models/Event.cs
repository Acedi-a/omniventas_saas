namespace SaaSEventos.Models;

public class Event
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public int MaxCapacity { get; set; }
    public int AvailableTickets { get; set; }
    public decimal Price { get; set; }
    public DateTime CreatedAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public List<OrderItem> OrderItems { get; set; } = new();
    public List<Ticket> Tickets { get; set; } = new();
}
