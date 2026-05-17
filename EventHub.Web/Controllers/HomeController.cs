using EventHub.Web.Data;
using EventHub.Web.Interfaces;
using EventHub.Web.Models;
using EventHub.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Web.Controllers;

public class HomeController(IEventService eventService, IDashboardService dashboardService, ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var events = await eventService.GetEventsAsync(new EventFilterViewModel());
        var dashboard = await dashboardService.GetDashboardAsync();

        ViewBag.TotalEvents = dashboard.TotalEvents;
        ViewBag.TotalUsers = dashboard.TotalUsers;
        ViewBag.Registrations = dashboard.Registrations;
        ViewBag.Revenue = dashboard.Revenue;

        return View(events);
    }

    public IActionResult Privacy() => View();

    [HttpGet("Contact")]
    public IActionResult Contact() => View(new ContactMessageViewModel());

    [HttpPost]
    [Route("Contact")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactMessageViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        context.ContactMessages.Add(new ContactMessage
        {
            FullName = model.FullName,
            Email = model.Email,
            Message = model.Message
        });
        context.AuditLogs.Add(new AuditLog
        {
            Action = "Contact Message Created",
            Details = model.Email,
            Actor = model.FullName
        });
        await context.SaveChangesAsync();

        TempData["Status"] = "Your message was sent successfully.";
        return RedirectToAction(nameof(Contact));
    }
}
