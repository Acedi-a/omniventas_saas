using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSEventos.DTOs.Tickets;
using SaaSEventos.Models.Enums;
using SaaSEventos.Services;

namespace SaaSEventos.Controllers;

[ApiController]
[Route("api/tickets")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly TicketService _ticketService;

    public TicketsController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet("my-tickets")]
    public async Task<IActionResult> GetMyTickets([FromQuery] TicketStatus? status)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new { error = "Authentication claims missing." });
        }

        var tickets = await _ticketService.GetMyTicketsAsync(userId, status);
        return Ok(tickets);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTicket(int id)
    {
        if (!TryGetUserId(out var userId))
        {
            return Unauthorized(new { error = "Authentication claims missing." });
        }

        var ticket = await _ticketService.GetTicketAsync(userId, id);
        if (ticket == null)
        {
            return NotFound(new { error = "Ticket not found." });
        }

        return Ok(ticket);
    }

    [HttpPost("validate")]
    [Authorize(Roles = "Validator")]
    public async Task<IActionResult> ValidateTicket(ValidateTicketRequest request)
    {
        if (!TryGetTenantId(out var tenantId))
        {
            return Unauthorized(new { error = "Authentication claims missing." });
        }

        var response = await _ticketService.ValidateTicketAsync(tenantId, request.Code);
        return Ok(response);
    }

    private bool TryGetUserId(out int userId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out userId);
    }

    private bool TryGetTenantId(out int tenantId)
    {
        var tenantIdClaim = User.FindFirst("TenantId")?.Value;
        return int.TryParse(tenantIdClaim, out tenantId);
    }
}
