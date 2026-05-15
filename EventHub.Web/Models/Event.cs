using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventHub.Web.Models;

public class Event
{
    public int Id { get; set; }

    [Required, StringLength(140)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1800)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(160)]
    public string Location { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, 100000)]
    public decimal Price { get; set; }

    [Range(1, 100000)]
    public int Capacity { get; set; }

    public DateTime EventDate { get; set; }

    [StringLength(600)]
    public string ImageUrl { get; set; } = "/images/event-placeholder.svg";

    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    [Required]
    public string OrganizerId { get; set; } = string.Empty;
    public ApplicationUser? Organizer { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Registration> Registrations { get; set; } = new List<Registration>();
}
