using System.ComponentModel.DataAnnotations;

namespace EventHub.Web.ViewModels;

public class ProfileEditViewModel
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Phone, StringLength(40)]
    public string? PhoneNumber { get; set; }
}
