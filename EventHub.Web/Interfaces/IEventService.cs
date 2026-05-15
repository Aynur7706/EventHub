using EventHub.Web.DTOs;
using EventHub.Web.ViewModels;

namespace EventHub.Web.Interfaces;

public interface IEventService
{
    Task<EventListViewModel> GetEventsAsync(EventFilterViewModel filter);
    Task<EventDto?> GetEventAsync(int id);
    Task<int> GetAvailableTicketsAsync(int eventId);
    Task<bool> RegisterAsync(int eventId, string userId, int ticketCount);
}
