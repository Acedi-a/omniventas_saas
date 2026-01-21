using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSEventos.DTOs.Coupons;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/coupons")]
[Authorize]
public class CouponsController : ControllerBase
{
    private readonly CouponService _couponService;

    public CouponsController(CouponService couponService)
    {
        _couponService = couponService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateCoupon(CreateCouponRequest request)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        var response = await _couponService.CreateCouponAsync(tenantId, request);
        return Ok(response);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCoupons()
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        var coupons = await _couponService.GetCouponsAsync(tenantId);
        return Ok(coupons);
    }

    [HttpPost("validate")]
    public async Task<IActionResult> ValidateCoupon(ValidateCouponRequest request)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "TenantId claim missing." });
        }

        var response = await _couponService.ValidateCouponAsync(tenantId, request.Code);
        return Ok(response);
    }

    private bool TryGetTenantId(out int tenantId)
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        return int.TryParse(tenantIdClaim, out tenantId);
    }
}
