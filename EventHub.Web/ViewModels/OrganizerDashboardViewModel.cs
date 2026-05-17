namespace EventHub.Web.ViewModels;

public class OrganizerDashboardViewModel
{
    public int TotalEvents { get; set; }
    public int PublishedEvents { get; set; }
    public int PendingEvents { get; set; }
    public int RejectedEvents { get; set; }
    public int TicketsSold { get; set; }
    public decimal Revenue { get; set; }
    public IReadOnlyList<OrganizerEventInsightViewModel> TopEvents { get; set; } = [];
    public IReadOnlyList<OrganizerEventInsightViewModel> ReviewQueue { get; set; } = [];
}

public class OrganizerEventInsightViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int Capacity { get; set; }
    public int TicketsSold { get; set; }
    public decimal Revenue { get; set; }
    public string? AdminNote { get; set; }
}
