namespace EventHub.Web.ViewModels;

public class DashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalEvents { get; set; }
    public int ActiveEvents { get; set; }
    public int Registrations { get; set; }
    public decimal Revenue { get; set; }
    public IReadOnlyList<DashboardEventRow> LatestEvents { get; set; } = [];
    public IReadOnlyList<DashboardRegistrationRow> LatestRegistrations { get; set; } = [];
    public IReadOnlyList<DashboardCategoryRow> CategoryBreakdown { get; set; } = [];
}

public record DashboardEventRow(int Id, string Title, DateTime EventDate, string Category, int Tickets, decimal Revenue);
public record DashboardRegistrationRow(int Id, string Customer, string EventTitle, int TicketCount, decimal TotalPrice, DateTime RegisteredAt);
public record DashboardCategoryRow(string Name, int EventCount, int TicketsSold);
