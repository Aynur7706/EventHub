using EventHub.Web.Data;
using EventHub.Web.Models;
using EventHub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

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
                r.Id,
                r.EventId,
                r.Event!.Title,
                r.Event.Category!.Name,
                r.Event.Location,
                r.Event.EventDate,
                r.TicketCount,
                r.TotalPrice,
                r.TicketCode))
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

    public async Task<IActionResult> Details(int id)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var ticket = await context.Registrations
            .Include(r => r.User)
            .Include(r => r.Event)
            .ThenInclude(e => e!.Category)
            .Where(r => r.Id == id && r.UserId == userId)
            .Select(r => new TicketDetailsViewModel
            {
                RegistrationId = r.Id,
                TicketCode = r.TicketCode,
                CustomerName = r.User!.FullName,
                CustomerEmail = r.User.Email ?? string.Empty,
                EventTitle = r.Event!.Title,
                Category = r.Event.Category!.Name,
                Location = r.Event.Location,
                EventDate = r.Event.EventDate,
                TicketCount = r.TicketCount,
                TotalPrice = r.TotalPrice,
                Status = r.Status,
                CheckedInAt = r.CheckedInAt
            })
            .FirstOrDefaultAsync();

        return ticket is null ? NotFound() : View(ticket);
    }

    public async Task<IActionResult> Qr(int id)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var ticketCode = await context.Registrations
            .Where(r => r.Id == id && r.UserId == userId)
            .Select(r => r.TicketCode)
            .FirstOrDefaultAsync();
        if (string.IsNullOrWhiteSpace(ticketCode))
        {
            return NotFound();
        }

        return Content(BuildQrSvg(ticketCode), "image/svg+xml", Encoding.UTF8);
    }

    private static string BuildQrSvg(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        var sb = new StringBuilder();
        sb.Append("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 29 29\" shape-rendering=\"crispEdges\">");
        sb.Append("<rect width=\"29\" height=\"29\" fill=\"#fff\"/>");
        AddFinder(sb, 1, 1);
        AddFinder(sb, 21, 1);
        AddFinder(sb, 1, 21);

        for (var y = 0; y < 29; y++)
        {
            for (var x = 0; x < 29; x++)
            {
                if (IsFinderArea(x, y))
                {
                    continue;
                }

                var index = (x * 31 + y * 17) % bytes.Length;
                if (((bytes[index] >> ((x + y) % 8)) & 1) == 1)
                {
                    sb.Append($"<rect x=\"{x}\" y=\"{y}\" width=\"1\" height=\"1\" fill=\"#050505\"/>");
                }
            }
        }

        sb.Append("</svg>");
        return sb.ToString();
    }

    private static void AddFinder(StringBuilder sb, int x, int y)
    {
        sb.Append($"<rect x=\"{x}\" y=\"{y}\" width=\"7\" height=\"7\" fill=\"#050505\"/>");
        sb.Append($"<rect x=\"{x + 1}\" y=\"{y + 1}\" width=\"5\" height=\"5\" fill=\"#fff\"/>");
        sb.Append($"<rect x=\"{x + 2}\" y=\"{y + 2}\" width=\"3\" height=\"3\" fill=\"#050505\"/>");
    }

    private static bool IsFinderArea(int x, int y) =>
        x is >= 1 and <= 7 && y is >= 1 and <= 7 ||
        x is >= 21 and <= 27 && y is >= 1 and <= 7 ||
        x is >= 1 and <= 7 && y is >= 21 and <= 27;
}
