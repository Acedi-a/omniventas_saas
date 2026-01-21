using Microsoft.EntityFrameworkCore;
using SaaSEventos.Data;
using SaaSEventos.DTOs.Events;
using SaaSEventos.Models;

namespace SaaSEventos.Services;

public class EventService
{
    private readonly AppDbContext _db;

    public EventService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<EventResponse>> GetEventsAsync(int tenantId, DateTime? date)
    {
        var query = _db.Events.Where(e => e.TenantId == tenantId);

        if (date.HasValue)
        {
            var start = date.Value.Date;
            var end = start.AddDays(1);
            query = query.Where(e => e.EventDate >= start && e.EventDate < end);
        }

        return await query
            .OrderBy(e => e.EventDate)
            .Select(e => new EventResponse
            {
                Id = e.Id,
                Name = e.Name,
                EventDate = e.EventDate,
                Location = e.Location,
                MaxCapacity = e.MaxCapacity,
                AvailableTickets = e.AvailableTickets,
                Price = e.Price,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<EventResponse?> GetEventAsync(int tenantId, int id)
    {
        return await _db.Events
            .Where(e => e.TenantId == tenantId && e.Id == id)
            .Select(e => new EventResponse
            {
                Id = e.Id,
                Name = e.Name,
                EventDate = e.EventDate,
                Location = e.Location,
                MaxCapacity = e.MaxCapacity,
                AvailableTickets = e.AvailableTickets,
                Price = e.Price,
                CreatedAt = e.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<EventResponse> CreateEventAsync(int tenantId, CreateEventRequest request)
    {
        var eventEntity = new Event
        {
            TenantId = tenantId,
            Name = request.Name,
            EventDate = request.EventDate,
            Location = request.Location,
            MaxCapacity = request.MaxCapacity,
            AvailableTickets = request.MaxCapacity,
            Price = request.Price,
            CreatedAt = DateTime.UtcNow
        };

        _db.Events.Add(eventEntity);
        await _db.SaveChangesAsync();

        return new EventResponse
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            EventDate = eventEntity.EventDate,
            Location = eventEntity.Location,
            MaxCapacity = eventEntity.MaxCapacity,
            AvailableTickets = eventEntity.AvailableTickets,
            Price = eventEntity.Price,
            CreatedAt = eventEntity.CreatedAt
        };
    }

    public async Task<EventResponse?> UpdateEventAsync(int tenantId, int id, UpdateEventRequest request)
    {
        var eventEntity = await _db.Events.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id);
        if (eventEntity == null)
        {
            return null;
        }

        eventEntity.Name = request.Name;
        eventEntity.EventDate = request.EventDate;
        eventEntity.Location = request.Location;
        eventEntity.MaxCapacity = request.MaxCapacity;
        eventEntity.Price = request.Price;

        if (eventEntity.AvailableTickets > request.MaxCapacity)
        {
            eventEntity.AvailableTickets = request.MaxCapacity;
        }

        await _db.SaveChangesAsync();

        return new EventResponse
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            EventDate = eventEntity.EventDate,
            Location = eventEntity.Location,
            MaxCapacity = eventEntity.MaxCapacity,
            AvailableTickets = eventEntity.AvailableTickets,
            Price = eventEntity.Price,
            CreatedAt = eventEntity.CreatedAt
        };
    }

    public async Task<bool> DeleteEventAsync(int tenantId, int id)
    {
        var eventEntity = await _db.Events.FirstOrDefaultAsync(e => e.TenantId == tenantId && e.Id == id);
        if (eventEntity == null)
        {
            return false;
        }

        _db.Events.Remove(eventEntity);
        await _db.SaveChangesAsync();
        return true;
    }
}
