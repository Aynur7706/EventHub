namespace EventHub.Web.ViewModels;

public class AuditLogViewModel
{
    public string Action { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
