namespace EventHub.Web.ViewModels;

public class AdminContactMessageViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Reply { get; set; }
    public string? RepliedBy { get; set; }
    public DateTime? RepliedAt { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}
