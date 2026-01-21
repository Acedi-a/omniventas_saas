using SaaSEventos.Models.Enums;

namespace SaaSEventos.Models;

public class Ticket
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int EventId { get; set; }
    public int UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string QRCodeUrl { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public DateTime? RedeemedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Order Order { get; set; } = null!;
    public Event Event { get; set; } = null!;
    public User User { get; set; } = null!;
}
