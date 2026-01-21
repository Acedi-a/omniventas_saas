namespace SaaSEventos.DTOs.Owner;

public class SlugAvailabilityResponse
{
    public bool Available { get; set; }
    public string NormalizedSlug { get; set; } = string.Empty;
}
