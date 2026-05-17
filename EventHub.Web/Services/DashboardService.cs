using EventHub.Web.Data;
using EventHub.Web.Constants;
using EventHub.Web.Interfaces;
using EventHub.Web.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Web.Services;

public class DashboardService(ApplicationDbContext context) : IDashboardService
{
    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var latestEvents = await context.Events
            .Include(e => e.Category)
            .Include(e => e.Registrations)
            .OrderByDescending(e => e.CreatedAt)
            .Take(6)
            .Select(e => new DashboardEventRow(
                e.Id,
                e.Title,
                e.EventDate,
                e.Category!.Name,
                e.Registrations.Sum(r => r.TicketCount),
                e.Registrations.Sum(r => r.TotalPrice)))
            .ToListAsync();

        var latestRegistrations = await context.Registrations
            .Include(r => r.User)
            .Include(r => r.Event)
            .OrderByDescending(r => r.RegisteredAt)
            .Take(6)
            .Select(r => new DashboardRegistrationRow(
                r.Id,
                r.User!.FullName,
                r.Event!.Title,
                r.TicketCount,
                r.TotalPrice,
                r.RegisteredAt))
            .ToListAsync();

        var categoryEvents = await context.Events
            .Include(e => e.Category)
            .Include(e => e.Registrations)
            .ToListAsync();

        var categoryBreakdown = categoryEvents
            .GroupBy(e => e.Category!.Name)
            .Select(g => new DashboardCategoryRow(
                g.Key,
                g.Count(),
                g.SelectMany(e => e.Registrations).Sum(r => r.TicketCount)))
            .OrderByDescending(c => c.TicketsSold)
            .ThenByDescending(c => c.EventCount)
            .Take(5)
            .ToList();

        return new DashboardViewModel
        {
            TotalUsers = await context.Users.CountAsync(),
            TotalEvents = await context.Events.CountAsync(),
            ActiveEvents = await context.Events.CountAsync(e => e.EventDate >= DateTime.Today && e.Status == EventStatuses.Published),
            Registrations = await context.Registrations.SumAsync(r => (int?)r.TicketCount) ?? 0,
            Revenue = await context.Registrations.SumAsync(r => (decimal?)r.TotalPrice) ?? 0,
            LatestEvents = latestEvents,
            LatestRegistrations = latestRegistrations,
            CategoryBreakdown = categoryBreakdown
        };
    }
}
