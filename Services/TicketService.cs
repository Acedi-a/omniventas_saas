using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.Tickets;
using SaaSEventos.Models.Enums;

namespace SaaSEventos.Services;

public class TicketService
{
    private readonly AppDbContext _db;

    public TicketService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<TicketSummaryResponse>> GetMyTicketsAsync(int userId, TicketStatus? status)
    {
        var query = _db.Tickets
            .Where(t => t.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TicketSummaryResponse
            {
                Id = t.Id,
                EventId = t.EventId,
                EventName = t.Event.Name,
                EventDate = t.Event.EventDate,
                Status = t.Status,
                QRCodeUrl = t.QRCodeUrl
            })
            .ToListAsync();
    }

    public async Task<TicketDetailResponse?> GetTicketAsync(int userId, int ticketId)
    {
        return await _db.Tickets
            .Where(t => t.UserId == userId && t.Id == ticketId)
            .Select(t => new TicketDetailResponse
            {
                Id = t.Id,
                Code = t.Code,
                QRCodeUrl = t.QRCodeUrl,
                Status = t.Status,
                EventName = t.Event.Name,
                EventDate = t.Event.EventDate,
                Location = t.Event.Location
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ValidateTicketResponse> ValidateTicketAsync(int tenantId, string code)
    {
        var ticket = await _db.Tickets
            .Include(t => t.Event)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Code == code);

        if (ticket == null || ticket.Event.TenantId != tenantId)
        {
            return new ValidateTicketResponse { Valid = false, Reason = "NOT_FOUND" };
        }

        if (ticket.Status != TicketStatus.Active)
        {
            return new ValidateTicketResponse { Valid = false, Reason = "ALREADY_USED" };
        }

        if (ticket.Event.EventDate <= DateTime.UtcNow)
        {
            return new ValidateTicketResponse { Valid = false, Reason = "EXPIRED" };
        }

        ticket.Status = TicketStatus.Redeemed;
        ticket.RedeemedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new ValidateTicketResponse
        {
            Valid = true,
            EventName = ticket.Event.Name,
            UserEmail = ticket.User.Email,
            RedeemedAt = ticket.RedeemedAt
        };
    }
}
