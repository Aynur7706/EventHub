using EventHub.Web.Interfaces;

namespace EventHub.Web.Services;

public class AiAssistantService(IConfiguration configuration) : IAiAssistantService
{
    public Task<string> GenerateSupportReplyAsync(string customerName, string email, string message)
    {
        var provider = configuration["AiAssistant:Provider"] ?? "Template";
        if (!provider.Equals("Template", StringComparison.OrdinalIgnoreCase))
        {
            // Future integration point for OpenAI or another provider. The app stays usable without an API key.
        }

        var topic = DetectTopic(message);
        var name = string.IsNullOrWhiteSpace(customerName) ? "there" : customerName.Trim();
        var reply = $"""
            Hello {name},

            Thank you for contacting EventHub. I reviewed your message about {topic}, and we will be happy to help.

            Based on your request, the next step is to confirm the event details, account email, and any specific dates or ticket information involved. If you can share those details, our team can guide you more accurately.

            Best regards,
            EventHub Support Team
            """;

        return Task.FromResult(reply);
    }

    private static string DetectTopic(string message)
    {
        var text = message.ToLowerInvariant();
        if (text.Contains("ticket") || text.Contains("bilet"))
        {
            return "ticket support";
        }

        if (text.Contains("organizer") || text.Contains("event") || text.Contains("tədbir"))
        {
            return "event organization";
        }

        if (text.Contains("login") || text.Contains("password") || text.Contains("account"))
        {
            return "account access";
        }

        return "your request";
    }
}
