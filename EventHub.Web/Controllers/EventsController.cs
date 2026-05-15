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
            IsRegistered = userId is not null && await context.Registrations.AnyAsync(r => r.EventId == id && r.UserId == userId)
        });
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
