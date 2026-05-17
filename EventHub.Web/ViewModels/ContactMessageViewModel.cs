using System.ComponentModel.DataAnnotations;

namespace EventHub.Web.ViewModels;

public class ContactMessageViewModel
{
    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(160)]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(1200)]
    public string Message { get; set; } = string.Empty;
}
