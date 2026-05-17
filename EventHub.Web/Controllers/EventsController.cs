using EventHub.Web.Data;
using EventHub.Web.Interfaces;
using EventHub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventHub.Web.Models;

namespace EventHub.Web.Controllers;

public class EventsController(IEventService eventService, ApplicationDbContext context, UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index([FromQuery] EventFilterViewModel filter)
    {
        return View(await eventService.GetEventsAsync(filter));
    }

    public async Task<IActionResult> Details(int id)
    {
        var item = await eventService.GetEventAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        var userId = userManager.GetUserId(User);
        return View(new EventDetailsViewModel
        {
            Event = item,
            AvailableTickets = await eventService.GetAvailableTicketsAsync(id),
            IsRegistered = userId is not null && await context.Registrations.AnyAsync(r => r.EventId == id && r.UserId == userId),
            IsSaved = userId is not null && await context.SavedEvents.AnyAsync(s => s.EventId == id && s.UserId == userId)
        });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(int id)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var exists = await context.SavedEvents.AnyAsync(s => s.EventId == id && s.UserId == userId);
        if (!exists)
        {
            context.SavedEvents.Add(new SavedEvent { EventId = id, UserId = userId });
            context.AuditLogs.Add(new AuditLog
            {
                Action = "Event Saved",
                Details = $"Event #{id}",
                Actor = User.Identity?.Name ?? "User"
            });
            await context.SaveChangesAsync();
            TempData["Status"] = "Event saved to your profile.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsave(int id)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var saved = await context.SavedEvents.FirstOrDefaultAsync(s => s.EventId == id && s.UserId == userId);
        if (saved is not null)
        {
            context.SavedEvents.Remove(saved);
            await context.SaveChangesAsync();
            TempData["Status"] = "Event removed from saved events.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int id, int ticketCount = 1)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var registered = await eventService.RegisterAsync(id, userId, ticketCount);
        TempData["Status"] = registered
            ? "Registration completed successfully. Your ticket is now visible in your calendar."
            : "Registration could not be completed. Please check availability.";

        if (registered)
        {
            return RedirectToAction("Calendar", "Tickets");
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
