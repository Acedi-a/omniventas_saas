namespace SaaSEventos.DTOs.Admin;

public class TenantActivityItem
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
    public decimal? Amount { get; set; }
    public int? ReferenceId { get; set; }
}
