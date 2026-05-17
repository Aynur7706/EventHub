using EventHub.Web.Constants;
using EventHub.Web.Data;
using EventHub.Web.Models;
using EventHub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class OrganizerRequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var requests = await context.OrganizerRequests
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new AdminOrganizerRequestViewModel
            {
                Id = r.Id,
                FullName = r.User!.FullName,
                Email = r.User.Email ?? string.Empty,
                OrganizationName = r.OrganizationName,
                Reason = r.Reason,
                Status = r.Status,
                AdminNote = r.AdminNote,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return View(requests);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var request = await context.OrganizerRequests.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
        if (request?.User is null)
        {
            return NotFound();
        }

        request.Status = OrganizerRequestStatuses.Approved;
        request.ReviewedAt = DateTime.UtcNow;
        if (!await userManager.IsInRoleAsync(request.User, AppRoles.Organizer))
        {
            await userManager.AddToRoleAsync(request.User, AppRoles.Organizer);
        }

        context.AuditLogs.Add(new AuditLog
        {
            Action = "Organizer Request Approved",
            Details = $"{request.User.Email} became Organizer",
            Actor = User.Identity?.Name ?? "Admin"
        });
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? adminNote)
    {
        var request = await context.OrganizerRequests.Include(r => r.User).FirstOrDefaultAsync(r => r.Id == id);
        if (request is null)
        {
            return NotFound();
        }

        request.Status = OrganizerRequestStatuses.Rejected;
        request.AdminNote = string.IsNullOrWhiteSpace(adminNote) ? "Request rejected by admin." : adminNote.Trim();
        request.ReviewedAt = DateTime.UtcNow;
        context.AuditLogs.Add(new AuditLog
        {
            Action = "Organizer Request Rejected",
            Details = request.User?.Email ?? request.OrganizationName,
            Actor = User.Identity?.Name ?? "Admin"
        });
        await context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
