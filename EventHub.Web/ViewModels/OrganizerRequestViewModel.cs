using System.ComponentModel.DataAnnotations;

namespace EventHub.Web.ViewModels;

public class OrganizerRequestViewModel
{
    [Required, StringLength(120)]
    public string OrganizationName { get; set; } = string.Empty;

    [Required, StringLength(1200)]
    public string Reason { get; set; } = string.Empty;
}
