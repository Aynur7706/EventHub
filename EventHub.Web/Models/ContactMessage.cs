using System.ComponentModel.DataAnnotations;
using EventHub.Web.Constants;

namespace EventHub.Web.Models;

public class ContactMessage
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(1200)]
    public string Message { get; set; } = string.Empty;

    [StringLength(40)]
    public string Status { get; set; } = ContactMessageStatuses.New;

    [StringLength(1600)]
    public string? Reply { get; set; }

    [StringLength(120)]
    public string? RepliedBy { get; set; }

    public DateTime? RepliedAt { get; set; }

    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
