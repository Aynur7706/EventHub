using EventHub.Web.DTOs;
using EventHub.Web.Interfaces;
using EventHub.Web.Models;
using EventHub.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Web.Services;

public class EventService(IUnitOfWork unitOfWork) : IEventService
{
    public async Task<EventListViewModel> GetEventsAsync(EventFilterViewModel filter)
    {
        var query = unitOfWork.Events.Query()
            .Include(e => e.Category)
            .Include(e => e.Organizer)
            .Include(e => e.Registrations)
            .Where(e => e.EventDate >= DateTime.Today)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            query = query.Where(e => e.Title.Contains(filter.Search) || e.Description.Contains(filter.Search));
        }

        if (!string.IsNullOrWhiteSpace(filter.Location))
        {
            query = query.Where(e => e.Location.Contains(filter.Location));
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(e => e.CategoryId == filter.CategoryId);
        }

        if (filter.DateFrom.HasValue)
        {
            query = query.Where(e => e.EventDate >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            query = query.Where(e => e.EventDate <= filter.DateTo.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            query = query.Where(e => e.Price <= filter.MaxPrice.Value);
        }

        var events = await query
            .OrderBy(e => e.EventDate)
            .Select(e => ToDto(e))
            .ToListAsync();

        return new EventListViewModel
        {
            Events = events,
            Categories = await unitOfWork.Categories.Query().OrderBy(c => c.Name).AsNoTracking().ToListAsync(),
            Filter = filter
        };
    }

    public async Task<EventDto?> GetEventAsync(int id)
    {
        var item = await unitOfWork.Events.Query()
            .Include(e => e.Category)
            .Include(e => e.Organizer)
            .Include(e => e.Registrations)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        return item is null ? null : ToDto(item);
    }

    public async Task<int> GetAvailableTicketsAsync(int eventId)
    {
        var item = await unitOfWork.Events.Query()
            .Include(e => e.Registrations)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == eventId);

        return item is null ? 0 : Math.Max(0, item.Capacity - item.Registrations.Sum(r => r.TicketCount));
    }

    public async Task<bool> RegisterAsync(int eventId, string userId, int ticketCount)
    {
        if (ticketCount < 1)
        {
            return false;
        }

        var item = await unitOfWork.Events.Query()
            .Include(e => e.Registrations)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (item is null || item.EventDate < DateTime.UtcNow || item.Registrations.Sum(r => r.TicketCount) + ticketCount > item.Capacity)
        {
            return false;
        }

        var existing = item.Registrations.FirstOrDefault(r => r.UserId == userId);
        if (existing is not null)
        {
            existing.TicketCount += ticketCount;
            existing.TotalPrice += item.Price * ticketCount;
        }
        else
        {
            await unitOfWork.Registrations.AddAsync(new Registration
            {
                EventId = eventId,
                UserId = userId,
                TicketCount = ticketCount,
                TotalPrice = item.Price * ticketCount
            });
        }

        await unitOfWork.SaveChangesAsync();
        return true;
    }

    private static EventDto ToDto(Event item) => new(
        item.Id,
        item.Title,
        item.Description,
        item.Location,
        item.Price,
        item.Capacity,
        item.EventDate,
        item.ImageUrl,
        item.Category?.Name ?? "Uncategorized",
        item.Organizer?.FullName ?? item.Organizer?.Email ?? "Organizer",
        item.Registrations.Sum(r => r.TicketCount));
}
