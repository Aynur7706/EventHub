namespace EventHub.Web.ViewModels;

public class AdminRegistrationViewModel
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string EventTitle { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int TicketCount { get; set; }
    public decimal TotalPrice { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? CheckedInAt { get; set; }
    public DateTime RegisteredAt { get; set; }
}
