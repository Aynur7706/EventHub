using EventHub.Web.Interfaces;
using EventHub.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Web.Controllers;

public class HomeController(IEventService eventService, IDashboardService dashboardService) : Controller
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

    public IActionResult Contact() => View();
}
