namespace EventHub.Web.ViewModels;

public class DashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalEvents { get; set; }
    public int ActiveEvents { get; set; }
    public int Registrations { get; set; }
    public decimal Revenue { get; set; }
    public IReadOnlyList<DashboardEventRow> LatestEvents { get; set; } = [];
}

public record DashboardEventRow(int Id, string Title, DateTime EventDate, string Category, int Tickets, decimal Revenue);
