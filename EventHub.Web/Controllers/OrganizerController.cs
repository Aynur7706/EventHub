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
            CurrentImageUrl = item.ImageUrl
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
