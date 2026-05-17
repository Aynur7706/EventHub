namespace EventHub.Web.ViewModels;

public class AdminCheckInViewModel
{
    public string TicketCode { get; set; } = string.Empty;
    public string? Message { get; set; }
    public bool IsSuccess { get; set; }
    public AdminRegistrationViewModel? Ticket { get; set; }
}
