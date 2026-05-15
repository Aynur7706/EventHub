using EventHub.Web.Constants;
using EventHub.Web.Data;
using EventHub.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class CategoriesController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index() => View(await context.Categories.OrderBy(c => c.Name).ToListAsync());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string name)
    {
        if (!string.IsNullOrWhiteSpace(name) && !await context.Categories.AnyAsync(c => c.Name == name))
        {
            context.Categories.Add(new Category { Name = name.Trim() });
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await context.Categories.FindAsync(id);
        if (category is not null)
        {
            context.Categories.Remove(category);
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
