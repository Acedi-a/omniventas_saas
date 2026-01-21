using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSEventos.DTOs.Owner;
using SaaSEventos.DTOs.Tenants;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/owner/tenants")]
[Authorize(Roles = "Owner")]
public class OwnerTenantsController : ControllerBase
{
    private readonly OwnerTenantService _tenantService;
    private readonly OwnerUserService _userService;

    public OwnerTenantsController(OwnerTenantService tenantService, OwnerUserService userService)
    {
        _tenantService = tenantService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTenants()
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Unauthorized(new { error = "AccountId claim missing." });
        }

        var tenants = await _tenantService.GetTenantsAsync(accountId);
        return Ok(tenants);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTenant(CreateTenantRequest request)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Unauthorized(new { error = "AccountId claim missing." });
        }

        var tenant = await _tenantService.CreateTenantAsync(accountId, request);
        return Ok(tenant);
    }

    [HttpGet("{id:int}/stats")]
    public async Task<IActionResult> GetTenantStats(int id)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Unauthorized(new { error = "AccountId claim missing." });
        }

        var stats = await _tenantService.GetTenantStatsAsync(accountId, id);
        if (stats == null)
        {
            return NotFound(new { error = "Tenant not found." });
        }

        return Ok(stats);
    }

    [HttpGet("{id:int}/users")]
    public async Task<IActionResult> GetUsers(int id)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Unauthorized(new { error = "AccountId claim missing." });
        }

        var users = await _userService.GetUsersAsync(accountId, id);
        if (users == null)
        {
            return NotFound(new { error = "Tenant not found." });
        }

        return Ok(users);
    }

    [HttpPost("{id:int}/users")]
    public async Task<IActionResult> CreateUser(int id, OwnerCreateUserRequest request)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Unauthorized(new { error = "AccountId claim missing." });
        }

        try
        {
            var user = await _userService.CreateUserAsync(accountId, id, request);
            if (user == null)
            {
                return NotFound(new { error = "Tenant not found." });
            }

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:int}/users/{userId:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id, int userId, ResetUserPasswordRequest request)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Unauthorized(new { error = "AccountId claim missing." });
        }

        try
        {
            var user = await _userService.ResetPasswordAsync(accountId, id, userId, request.NewPassword);
            if (user == null)
            {
                return NotFound(new { error = "User not found." });
            }

            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id:int}/slug")]
    public async Task<IActionResult> UpdateSlug(int id, UpdateTenantSlugRequest request)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Unauthorized(new { error = "AccountId claim missing." });
        }

        try
        {
            var tenant = await _tenantService.UpdateSlugAsync(accountId, id, request.Slug);
            if (tenant == null)
            {
                return NotFound(new { error = "Tenant not found." });
            }

            return Ok(tenant);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:int}/slug-availability")]
    public async Task<IActionResult> CheckSlug(int id, [FromQuery] string slug)
    {
        if (!TryGetAccountId(out var accountId))
        {
            return Unauthorized(new { error = "AccountId claim missing." });
        }

        var availability = await _tenantService.CheckSlugAvailabilityAsync(accountId, id, slug);
        return Ok(availability);
    }

    private bool TryGetAccountId(out int accountId)
    {
        var claim = User.FindFirst("AccountId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(claim, out accountId);
    }
}
