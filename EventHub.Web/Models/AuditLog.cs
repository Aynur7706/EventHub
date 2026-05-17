using System.ComponentModel.DataAnnotations;

namespace EventHub.Web.Models;

public class AuditLog
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Action { get; set; } = string.Empty;

    [Required, StringLength(220)]
    public string Details { get; set; } = string.Empty;

    [StringLength(120)]
    public string Actor { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
