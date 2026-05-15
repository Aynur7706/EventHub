using EventHub.Web.Data;
using EventHub.Web.Models;
using EventHub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Web.Controllers;

[Authorize]
public class TicketsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Calendar(int? month, int? year)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var today = DateTime.Today;
        var currentMonth = new DateTime(year ?? today.Year, month ?? today.Month, 1);
        var firstVisibleDay = currentMonth.AddDays(-(int)currentMonth.DayOfWeek);
        var lastVisibleDay = firstVisibleDay.AddDays(41);

        var registrations = await context.Registrations
            .Include(r => r.Event)
            .ThenInclude(e => e!.Category)
            .Where(r => r.UserId == userId && r.Event != null)
            .OrderBy(r => r.Event!.EventDate)
            .Select(r => new TicketCalendarEvent(
                r.EventId,
                r.Event!.Title,
                r.Event.Category!.Name,
                r.Event.Location,
                r.Event.EventDate,
                r.TicketCount,
                r.TotalPrice))
            .ToListAsync();

        var eventsByDate = registrations
            .Where(e => e.EventDate.Date >= firstVisibleDay.Date && e.EventDate.Date <= lastVisibleDay.Date)
            .GroupBy(e => e.EventDate.Date)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<TicketCalendarEvent>)g.ToList());

        var days = Enumerable.Range(0, 42)
            .Select(offset =>
            {
                var date = firstVisibleDay.AddDays(offset);
                return new TicketCalendarDay
                {
                    Date = date,
                    IsCurrentMonth = date.Month == currentMonth.Month,
                    IsToday = date.Date == today,
                    Events = eventsByDate.TryGetValue(date.Date, out var events) ? events : []
                };
            })
            .ToList();

        return View(new TicketCalendarViewModel
        {
            CurrentMonth = currentMonth,
            Days = days,
            UpcomingEvents = registrations.Where(e => e.EventDate.Date >= today).Take(8).ToList()
        });
    }
}
