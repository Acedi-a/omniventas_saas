namespace SaaSEventos.DTOs.Coupons;

public class CouponResponse
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal DiscountPercentage { get; set; }
    public int MaxUses { get; set; }
    public int CurrentUses { get; set; }
    public DateTime ExpiresAt { get; set; }
}
