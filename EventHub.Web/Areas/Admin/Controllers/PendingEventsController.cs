using EventHub.Web.Constants;
using EventHub.Web.Data;
using EventHub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class PendingEventsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = await context.Events
            .Include(e => e.Organizer)
            .Include(e => e.Category)
            .Include(e => e.Registrations)
            .Where(e => e.Status == EventStatuses.PendingReview || e.Status == EventStatuses.Rejected)
            .OrderByDescending(e => e.CreatedAt)
            .Select(e => new AdminPendingEventViewModel
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Location = e.Location,
                Price = e.Price,
                Capacity = e.Capacity,
                EventDate = e.EventDate,
                ImageUrl = e.ImageUrl,
                Category = e.Category!.Name,
                OrganizerName = e.Organizer!.FullName,
                OrganizerEmail = e.Organizer.Email ?? string.Empty,
                Status = e.Status,
                AdminNote = e.AdminNote,
                TicketsSold = e.Registrations.Sum(r => r.TicketCount),
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return View(model);
    }
}
