using EventHub.Web.DTOs;
using EventHub.Web.Models;

namespace EventHub.Web.ViewModels;

public class EventListViewModel
{
    public IReadOnlyList<EventDto> Events { get; set; } = [];
    public IReadOnlyList<Category> Categories { get; set; } = [];
    public EventFilterViewModel Filter { get; set; } = new();
}
