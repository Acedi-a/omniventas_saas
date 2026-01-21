namespace SaaSEventos.Models;

public class Coupon
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; }
    public int MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public DateTime ExpiresAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
}
