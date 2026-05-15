using System.ComponentModel.DataAnnotations.Schema;

namespace EventHub.Web.Models;

public class Registration
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public int EventId { get; set; }
    public Event? Event { get; set; }

    public int TicketCount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
}
