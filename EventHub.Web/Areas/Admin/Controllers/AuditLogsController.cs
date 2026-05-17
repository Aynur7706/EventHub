using EventHub.Web.Constants;
using EventHub.Web.Data;
using EventHub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class AuditLogsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = await context.AuditLogs
            .OrderByDescending(l => l.CreatedAt)
            .Take(100)
            .Select(l => new AuditLogViewModel
            {
                Action = l.Action,
                Details = l.Details,
                Actor = l.Actor,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();

        return View(model);
    }
}
