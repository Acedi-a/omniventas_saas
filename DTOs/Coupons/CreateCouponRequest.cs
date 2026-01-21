namespace SaaSEventos.DTOs.Coupons;

public class CreateCouponRequest
{
    public string Code { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; }
    public int MaxUses { get; set; }
    public DateTime ExpiresAt { get; set; }
}
