using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSEventos.DTOs.Admin;
using SaaSEventos.DTOs.Tenants;
using SaaSEventos.Models.Enums;
using SaaSEventos.Services;
using System.Text;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/admin/tenants")]
[Authorize(Roles = "SuperAdmin")]
public class AdminTenantsController : ControllerBase
{
    private readonly TenantService _tenantService;

    public AdminTenantsController(TenantService tenantService)
    {
        _tenantService = tenantService;
    }

    [HttpGet]
    public async Task<IActionResult> GetTenants(
        [FromQuery] string? search,
        [FromQuery] BusinessType? businessType,
        [FromQuery] bool? isActive,
        [FromQuery] DateTime? createdFrom,
        [FromQuery] DateTime? createdTo,
        [FromQuery] decimal? minSales,
        [FromQuery] decimal? maxSales,
        [FromQuery] int? minOrders,
        [FromQuery] int? maxOrders,
        [FromQuery] int? activityDays,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > 50)
        {
            pageSize = 10;
        }

        var tenants = await _tenantService.GetTenantsPagedAsync(
            search,
            businessType,
            isActive,
            createdFrom,
            createdTo,
            minSales,
            maxSales,
            minOrders,
            maxOrders,
            activityDays,
            page,
            pageSize);
        return Ok(tenants);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var summary = await _tenantService.GetSummaryAsync();
        return Ok(summary);
    }

    [HttpGet("trends")]
    public async Task<IActionResult> GetTrends([FromQuery] int days = 30)
    {
        var trends = await _tenantService.GetTenantTrendsAsync(days);
        return Ok(trends);
    }

    [HttpGet("recent")]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 5)
    {
        var recent = await _tenantService.GetRecentTenantsAsync(limit);
        return Ok(recent);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTenant(int id)
    {
        var tenant = await _tenantService.GetTenantAsync(id);
        if (tenant == null)
        {
            return NotFound(new { error = "Tenant not found." });
        }

        return Ok(tenant);
    }

    [HttpGet("{id:int}/stats")]
    public async Task<IActionResult> GetTenantStats(int id)
    {
        var stats = await _tenantService.GetTenantStatsAsync(id);
        if (stats == null)
        {
            return NotFound(new { error = "Tenant not found." });
        }

        return Ok(stats);
    }

    [HttpGet("{id:int}/activity")]
    public async Task<IActionResult> GetTenantActivity(int id, [FromQuery] int limit = 10)
    {
        var tenant = await _tenantService.GetTenantAsync(id);
        if (tenant == null)
        {
            return NotFound(new { error = "Tenant not found." });
        }

        var activity = await _tenantService.GetTenantActivityAsync(id, limit);
        return Ok(activity);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTenant(CreateTenantRequest request)
    {
        var response = await _tenantService.CreateTenantAsync(request);
        return Ok(response);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateTenantStatusRequest request)
    {
        var tenant = await _tenantService.UpdateTenantStatusAsync(id, request.IsActive);
        if (tenant == null)
        {
            return NotFound(new { error = "Tenant not found." });
        }

        return Ok(tenant);
    }

    [HttpPost("{id:int}/regenerate-api-key")]
    public async Task<IActionResult> RegenerateApiKey(int id)
    {
        var tenant = await _tenantService.RegenerateApiKeyAsync(id);
        if (tenant == null)
        {
            return NotFound(new { error = "Tenant not found." });
        }

        return Ok(tenant);
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportTenants(
        [FromQuery] string? search,
        [FromQuery] BusinessType? businessType,
        [FromQuery] bool? isActive,
        [FromQuery] DateTime? createdFrom,
        [FromQuery] DateTime? createdTo,
        [FromQuery] decimal? minSales,
        [FromQuery] decimal? maxSales,
        [FromQuery] int? minOrders,
        [FromQuery] int? maxOrders,
        [FromQuery] int? activityDays)
    {
        var tenants = await _tenantService.GetTenantsExportAsync(
            search,
            businessType,
            isActive,
            createdFrom,
            createdTo,
            minSales,
            maxSales,
            minOrders,
            maxOrders,
            activityDays);

        var builder = new StringBuilder();
        builder.AppendLine("Id,Name,BusinessType,IsActive,CreatedAt,Users,Orders,PaidOrders,OrdersLast30Days,TotalSales,LastOrderAt,LastActivityAt");
        foreach (var tenant in tenants)
        {
            builder.AppendLine(
                $"{tenant.Id},\"{tenant.Name}\",{tenant.BusinessType},{tenant.IsActive},{tenant.CreatedAt:O},{tenant.UsersCount},{tenant.OrdersCount},{tenant.PaidOrdersCount},{tenant.OrdersLast30Days},{tenant.TotalSales},{tenant.LastOrderAt:O},{tenant.LastActivityAt:O}");
        }

        var bytes = Encoding.UTF8.GetBytes(builder.ToString());
        return File(bytes, "text/csv", "tenants-export.csv");
    }
}
