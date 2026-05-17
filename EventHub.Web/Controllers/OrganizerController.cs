using EventHub.Web.Constants;
using EventHub.Web.Data;
using EventHub.Web.Interfaces;
using EventHub.Web.Models;
using EventHub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Web.Controllers;

[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Organizer}")]
public class OrganizerController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IImageStorageService imageStorage) : Controller
{
    public async Task<IActionResult> Dashboard()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var query = context.Events.Include(e => e.Registrations).AsQueryable();
        if (!User.IsInRole(AppRoles.Admin))
        {
            query = query.Where(e => e.OrganizerId == userId);
        }

        var events = await query.AsNoTracking().ToListAsync();
        var insights = events.Select(e => new OrganizerEventInsightViewModel
        {
            Id = e.Id,
            Title = e.Title,
            Status = e.Status,
            EventDate = e.EventDate,
            Capacity = e.Capacity,
            TicketsSold = e.Registrations.Sum(r => r.TicketCount),
            Revenue = e.Registrations.Sum(r => r.TotalPrice),
            AdminNote = e.AdminNote
        }).ToList();

        return View(new OrganizerDashboardViewModel
        {
            TotalEvents = events.Count,
            PublishedEvents = events.Count(e => e.Status == EventStatuses.Published),
            PendingEvents = events.Count(e => e.Status == EventStatuses.PendingReview),
            RejectedEvents = events.Count(e => e.Status == EventStatuses.Rejected),
            TicketsSold = insights.Sum(e => e.TicketsSold),
            Revenue = insights.Sum(e => e.Revenue),
            TopEvents = insights.OrderByDescending(e => e.Revenue).ThenByDescending(e => e.TicketsSold).Take(5).ToList(),
            ReviewQueue = insights
                .Where(e => e.Status is EventStatuses.PendingReview or EventStatuses.Rejected)
                .OrderByDescending(e => e.EventDate)
                .Take(5)
                .ToList()
        });
    }

    public async Task<IActionResult> Index()
    {
        var userId = userManager.GetUserId(User);
        var query = context.Events.Include(e => e.Category).Include(e => e.Registrations).AsQueryable();
        if (!User.IsInRole(AppRoles.Admin))
        {
            query = query.Where(e => e.OrganizerId == userId);
        }

        return View(await query.OrderByDescending(e => e.CreatedAt).ToListAsync());
    }

    public async Task<IActionResult> Create() => View(await BuildFormAsync(new EventFormViewModel()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model));
        }

        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        model.Status = User.IsInRole(AppRoles.Admin) ? EventStatuses.Published : EventStatuses.PendingReview;
        var imageUrl = await imageStorage.SaveAsync(model.Image, "/images/event-placeholder.svg");
        context.Events.Add(model.ToEntity(userId, imageUrl));
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var item = await FindOwnedEventAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        return View(await BuildFormAsync(new EventFormViewModel
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            Location = item.Location,
            Price = item.Price,
            Capacity = item.Capacity,
            EventDate = item.EventDate,
            CategoryId = item.CategoryId,
            CurrentImageUrl = item.ImageUrl,
            Status = item.Status
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EventFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var item = await FindOwnedEventAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildFormAsync(model));
        }

        item.Title = model.Title;
        item.Description = model.Description;
        item.Location = model.Location;
        item.Price = model.Price;
        item.Capacity = model.Capacity;
        item.EventDate = model.EventDate;
        item.CategoryId = model.CategoryId;
        item.ImageUrl = await imageStorage.SaveAsync(model.Image, item.ImageUrl);
        item.Status = User.IsInRole(AppRoles.Admin) ? item.Status : EventStatuses.PendingReview;

        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Publish(int id)
    {
        var item = await context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.Status = EventStatuses.Published;
        item.AdminNote = null;
        context.AuditLogs.Add(new AuditLog
        {
            Action = "Event Published",
            Details = item.Title,
            Actor = User.Identity?.Name ?? "Admin"
        });
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> Reject(int id, string? adminNote)
    {
        var item = await context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.Status = EventStatuses.Rejected;
        item.AdminNote = string.IsNullOrWhiteSpace(adminNote)
            ? "Event was rejected by admin."
            : adminNote.Trim();
        context.AuditLogs.Add(new AuditLog
        {
            Action = "Event Rejected",
            Details = item.Title,
            Actor = User.Identity?.Name ?? "Admin"
        });
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.Admin)]
    public async Task<IActionResult> MoveToDraft(int id)
    {
        var item = await context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (item is null)
        {
            return NotFound();
        }

        item.Status = EventStatuses.Draft;
        item.AdminNote = "Moved back to draft by admin.";
        context.AuditLogs.Add(new AuditLog
        {
            Action = "Event Moved To Draft",
            Details = item.Title,
            Actor = User.Identity?.Name ?? "Admin"
        });
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await FindOwnedEventAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        context.Events.Remove(item);
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<Event?> FindOwnedEventAsync(int id)
    {
        var userId = userManager.GetUserId(User);
        var query = context.Events.AsQueryable();
        if (!User.IsInRole(AppRoles.Admin))
        {
            query = query.Where(e => e.OrganizerId == userId);
        }

        return await query.FirstOrDefaultAsync(e => e.Id == id);
    }

    private async Task<EventFormViewModel> BuildFormAsync(EventFormViewModel model)
    {
        model.Categories = await context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToListAsync();
        return model;
    }
}
