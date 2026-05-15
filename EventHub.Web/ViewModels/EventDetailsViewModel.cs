using EventHub.Web.DTOs;

namespace EventHub.Web.ViewModels;

public class EventDetailsViewModel
{
    public EventDto Event { get; set; } = default!;
    public int AvailableTickets { get; set; }
    public bool IsRegistered { get; set; }
}
