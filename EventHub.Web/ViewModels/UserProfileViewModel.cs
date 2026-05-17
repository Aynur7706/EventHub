namespace EventHub.Web.ViewModels;

public class UserProfileViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<UserProfileTicketViewModel> Tickets { get; set; } = [];
    public IReadOnlyList<UserSavedEventViewModel> SavedEvents { get; set; } = [];
}

public class UserProfileTicketViewModel
{
    public int RegistrationId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public int TicketCount { get; set; }
    public decimal TotalPrice { get; set; }
    public string TicketCode { get; set; } = string.Empty;
}

public class UserSavedEventViewModel
{
    public int SavedEventId { get; set; }
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
}
