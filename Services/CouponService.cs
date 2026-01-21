using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.Coupons;
using SaaSEventos.Models;

namespace SaaSEventos.Services;

public class CouponService
{
    private readonly AppDbContext _db;

    public CouponService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CouponResponse> CreateCouponAsync(int tenantId, CreateCouponRequest request)
    {
        var coupon = new Coupon
        {
            TenantId = tenantId,
            Code = request.Code,
            DiscountPercentage = request.DiscountPercentage,
            MaxUses = request.MaxUses,
            CurrentUses = 0,
            ExpiresAt = request.ExpiresAt
        };

        _db.Coupons.Add(coupon);
        await _db.SaveChangesAsync();

        return new CouponResponse
        {
            Id = coupon.Id,
            Code = coupon.Code,
            DiscountPercentage = coupon.DiscountPercentage,
            MaxUses = coupon.MaxUses,
            CurrentUses = coupon.CurrentUses,
            ExpiresAt = coupon.ExpiresAt
        };
    }

    public async Task<ValidateCouponResponse> ValidateCouponAsync(int tenantId, string code)
    {
        var coupon = await _db.Coupons
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Code == code);

        if (coupon == null)
        {
            return new ValidateCouponResponse { Valid = false, Reason = "NOT_FOUND" };
        }

        if (coupon.ExpiresAt <= DateTime.UtcNow)
        {
            return new ValidateCouponResponse { Valid = false, Reason = "EXPIRED" };
        }

        if (coupon.CurrentUses >= coupon.MaxUses)
        {
            return new ValidateCouponResponse { Valid = false, Reason = "MAX_USES" };
        }

        return new ValidateCouponResponse
        {
            Valid = true,
            DiscountPercentage = coupon.DiscountPercentage,
            ExpiresAt = coupon.ExpiresAt
        };
    }

    public async Task<decimal> ValidateAndApplyAsync(int tenantId, string code, decimal subtotal)
    {
        var coupon = await _db.Coupons
            .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Code == code);

        if (coupon == null)
        {
            throw new InvalidOperationException("Coupon not found.");
        }

        if (coupon.ExpiresAt <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Coupon expired.");
        }

        if (coupon.CurrentUses >= coupon.MaxUses)
        {
            throw new InvalidOperationException("Coupon max uses reached.");
        }

        var discount = subtotal * (coupon.DiscountPercentage / 100m);
        coupon.CurrentUses += 1;
        return discount;
    }
}
