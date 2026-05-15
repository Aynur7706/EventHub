using EventHub.Web.Data;
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

        return new DashboardViewModel
        {
            TotalUsers = await context.Users.CountAsync(),
            TotalEvents = await context.Events.CountAsync(),
            ActiveEvents = await context.Events.CountAsync(e => e.EventDate >= DateTime.Today),
            Registrations = await context.Registrations.SumAsync(r => (int?)r.TicketCount) ?? 0,
            Revenue = await context.Registrations.SumAsync(r => (decimal?)r.TotalPrice) ?? 0,
            LatestEvents = latestEvents
        };
    }
}
