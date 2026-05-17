namespace EventHub.Web.ViewModels;

public class TicketCalendarViewModel
{
    public DateTime CurrentMonth { get; set; }
    public IReadOnlyList<TicketCalendarDay> Days { get; set; } = [];
    public IReadOnlyList<TicketCalendarEvent> UpcomingEvents { get; set; } = [];
}

public class TicketCalendarDay
{
    public DateTime Date { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }
    public IReadOnlyList<TicketCalendarEvent> Events { get; set; } = [];
}

public record TicketCalendarEvent(
    int RegistrationId,
    int EventId,
    string Title,
    string Category,
    string Location,
    DateTime EventDate,
    int TicketCount,
    decimal TotalPrice,
    string TicketCode);
