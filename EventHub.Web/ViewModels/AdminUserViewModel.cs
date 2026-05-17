namespace EventHub.Web.ViewModels;

public class AdminUserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
    public int Tickets { get; set; }
    public int OrganizedEvents { get; set; }
    public DateTime CreatedAt { get; set; }
}
