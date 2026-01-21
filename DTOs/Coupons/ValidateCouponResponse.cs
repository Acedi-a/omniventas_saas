namespace SaaSEventos.DTOs.Coupons;

public class ValidateCouponResponse
{
    public bool Valid { get; set; }
    public string? Reason { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
