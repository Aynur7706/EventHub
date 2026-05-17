using System.ComponentModel.DataAnnotations;
using EventHub.Web.Constants;

namespace EventHub.Web.Models;

public class OrganizerRequest
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Required, StringLength(120)]
    public string OrganizationName { get; set; } = string.Empty;

    [Required, StringLength(1200)]
    public string Reason { get; set; } = string.Empty;

    [StringLength(40)]
    public string Status { get; set; } = OrganizerRequestStatuses.Pending;

    [StringLength(700)]
    public string? AdminNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
}
