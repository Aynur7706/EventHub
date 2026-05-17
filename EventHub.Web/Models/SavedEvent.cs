namespace EventHub.Web.Models;

public class SavedEvent
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public DateTime SavedAt { get; set; } = DateTime.UtcNow;
}
