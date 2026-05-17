namespace EventHub.Web.DTOs;

public record EventDto(
    int Id,
    string Title,
    string Description,
    string Location,
    decimal Price,
    int Capacity,
    DateTime EventDate,
    string ImageUrl,
    string CategoryName,
    string OrganizerName,
    int RegisteredTickets,
    string Status);
