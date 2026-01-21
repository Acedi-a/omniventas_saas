using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSEventos.DTOs.Orders;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        if (!TryGetUserAndTenant(out var userId, out var tenantId))
        {
            return Unauthorized(new { error = "Authentication claims missing." });
        }

        try
        {
            var response = await _orderService.CreateOrderAsync(tenantId, userId, request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        if (!TryGetUserAndTenant(out var userId, out var tenantId))
        {
            return Unauthorized(new { error = "Authentication claims missing." });
        }

        var order = await _orderService.GetOrderAsync(tenantId, userId, id);
        if (order == null)
        {
            return NotFound(new { error = "Order not found." });
        }

        return Ok(order);
    }

    [HttpGet("my-orders")]
    public async Task<IActionResult> GetMyOrders()
    {
        if (!TryGetUserAndTenant(out var userId, out var tenantId))
        {
            return Unauthorized(new { error = "Authentication claims missing." });
        }

        var orders = await _orderService.GetMyOrdersAsync(tenantId, userId);
        return Ok(orders);
    }

    [HttpPost("{id:int}/confirm-payment")]
    public async Task<IActionResult> ConfirmPayment(int id)
    {
        if (!TryGetUserAndTenant(out var userId, out var tenantId))
        {
            return Unauthorized(new { error = "Authentication claims missing." });
        }

        try
        {
            var response = await _orderService.ConfirmPaymentAsync(tenantId, userId, id);
            if (response == null)
            {
                return NotFound(new { error = "Order not found." });
            }

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    private bool TryGetUserAndTenant(out int userId, out int tenantId)
    {
        userId = 0;
        tenantId = 0;

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;

        return int.TryParse(userIdClaim, out userId) && int.TryParse(tenantIdClaim, out tenantId);
    }
}
