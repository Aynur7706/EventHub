using EventHub.Web.Constants;
using EventHub.Web.Data;
using EventHub.Web.Models;
using EventHub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Web.Controllers;

[Authorize]
public class OrganizerRequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Create()
    {
        var userId = userManager.GetUserId(User);
        if (userId is null)
        {
            return Challenge();
        }

        ViewBag.ExistingRequest = await context.OrganizerRequests
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .FirstOrDefaultAsync();

        return View(new OrganizerRequestViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrganizerRequestViewModel model)
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

        var hasPending = await context.OrganizerRequests
            .AnyAsync(r => r.UserId == userId && r.Status == OrganizerRequestStatuses.Pending);
        if (hasPending)
        {
            TempData["Status"] = "You already have a pending organizer request.";
            return RedirectToAction(nameof(Create));
        }

        context.OrganizerRequests.Add(new OrganizerRequest
        {
            UserId = userId,
            OrganizationName = model.OrganizationName,
            Reason = model.Reason
        });
        context.AuditLogs.Add(new AuditLog
        {
            Action = "Organizer Request Created",
            Details = model.OrganizationName,
            Actor = User.Identity?.Name ?? "User"
        });
        await context.SaveChangesAsync();

        TempData["Status"] = "Your organizer request was sent to admin review.";
        return RedirectToAction(nameof(Create));
    }
}
