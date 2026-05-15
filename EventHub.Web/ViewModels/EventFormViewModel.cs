using System.ComponentModel.DataAnnotations;
using EventHub.Web.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EventHub.Web.ViewModels;

public class EventFormViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(140)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1800)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(160)]
    public string Location { get; set; } = string.Empty;

    [Range(0, 100000)]
    public decimal Price { get; set; }

    [Range(1, 100000)]
    public int Capacity { get; set; } = 100;

    [DataType(DataType.DateTime)]
    public DateTime EventDate { get; set; } = DateTime.Now.AddDays(14);

    [Required]
    public int CategoryId { get; set; }

    public string? CurrentImageUrl { get; set; }
    public IFormFile? Image { get; set; }
    public IReadOnlyList<SelectListItem> Categories { get; set; } = [];

    public Event ToEntity(string organizerId, string imageUrl) => new()
    {
        Title = Title,
        Description = Description,
        Location = Location,
        Price = Price,
        Capacity = Capacity,
        EventDate = EventDate,
        CategoryId = CategoryId,
        OrganizerId = organizerId,
        ImageUrl = imageUrl
    };
}
