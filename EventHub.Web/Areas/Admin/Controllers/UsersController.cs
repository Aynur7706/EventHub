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
public class UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var users = await context.Users
            .Include(u => u.Registrations)
            .Include(u => u.OrganizedEvents)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        var model = new List<AdminUserViewModel>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            model.Add(new AdminUserViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? user.UserName ?? string.Empty,
                Roles = string.Join(", ", roles),
                Tickets = user.Registrations.Sum(r => r.TicketCount),
                OrganizedEvents = user.OrganizedEvents.Count,
                CreatedAt = user.CreatedAt
            });
        }

        return View(model);
    }
}
