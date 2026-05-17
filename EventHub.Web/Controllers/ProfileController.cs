using EventHub.Web.Data;
using EventHub.Web.Models;
using EventHub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Web.Controllers;

[Authorize]
public class ProfileController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null)
        {
            return NotFound();
        }

        var tickets = await context.Registrations
            .Include(r => r.Event)
            .ThenInclude(e => e!.Category)
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.Event!.EventDate)
            .Select(r => new UserProfileTicketViewModel
            {
                RegistrationId = r.Id,
                EventTitle = r.Event!.Title,
                Category = r.Event.Category!.Name,
                Location = r.Event.Location,
                EventDate = r.Event.EventDate,
                TicketCount = r.TicketCount,
                TotalPrice = r.TotalPrice,
                TicketCode = r.TicketCode
            })
            .ToListAsync();

        var savedEvents = await context.SavedEvents
            .Include(s => s.Event)
            .ThenInclude(e => e!.Category)
            .Where(s => s.UserId == userId && s.Event != null)
            .OrderBy(s => s.Event!.EventDate)
            .Select(s => new UserSavedEventViewModel
            {
                SavedEventId = s.Id,
                EventId = s.EventId,
                Title = s.Event!.Title,
                Category = s.Event.Category!.Name,
                Location = s.Event.Location,
                EventDate = s.Event.EventDate,
                ImageUrl = s.Event.ImageUrl
            })
            .ToListAsync();

        return View(new UserProfileViewModel
        {
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            CreatedAt = user.CreatedAt,
            Tickets = tickets,
            SavedEvents = savedEvents
        });
    }

    public async Task<IActionResult> Edit()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        return View(new ProfileEditViewModel
        {
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileEditViewModel model)
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        user.FullName = model.FullName.Trim();
        user.PhoneNumber = model.PhoneNumber?.Trim();
        await userManager.UpdateAsync(user);

        context.AuditLogs.Add(new AuditLog
        {
            Action = "Profile Updated",
            Details = user.Email ?? user.UserName ?? user.Id,
            Actor = user.Email ?? "User"
        });
        await context.SaveChangesAsync();

        TempData["ProfileStatus"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
