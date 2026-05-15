using System.ComponentModel.DataAnnotations;

namespace EventHub.Web.Models;

public class Category
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
