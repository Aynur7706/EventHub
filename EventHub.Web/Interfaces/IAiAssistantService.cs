namespace EventHub.Web.Interfaces;

public interface IAiAssistantService
{
    Task<string> GenerateSupportReplyAsync(string customerName, string email, string message);
}
