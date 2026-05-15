using EventHub.Web.Constants;
using EventHub.Web.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class DashboardController(IDashboardService dashboardService) : Controller
{
    public async Task<IActionResult> Index() => View(await dashboardService.GetDashboardAsync());
}
