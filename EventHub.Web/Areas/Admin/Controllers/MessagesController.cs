using EventHub.Web.Constants;
using EventHub.Web.Data;
using EventHub.Web.Interfaces;
using EventHub.Web.Models;
using EventHub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class MessagesController(ApplicationDbContext context, IAiAssistantService aiAssistant) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = await context.ContactMessages
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new AdminContactMessageViewModel
            {
                Id = m.Id,
                FullName = m.FullName,
                Email = m.Email,
                Message = m.Message,
                Status = m.Status,
                Reply = m.Reply,
                RepliedBy = m.RepliedBy,
                RepliedAt = m.RepliedAt,
                IsRead = m.IsRead,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var message = await context.ContactMessages.FindAsync(id);
        if (message is not null)
        {
            message.IsRead = true;
            if (message.Status == ContactMessageStatuses.New)
            {
                message.Status = ContactMessageStatuses.Read;
            }
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> GenerateDraft(int id)
    {
        var message = await context.ContactMessages.FindAsync(id);
        if (message is null)
        {
            return NotFound();
        }

        message.Reply = await aiAssistant.GenerateSupportReplyAsync(message.FullName, message.Email, message.Message);
        if (message.Status == ContactMessageStatuses.New)
        {
            message.Status = ContactMessageStatuses.Read;
        }

        message.IsRead = true;
        context.AuditLogs.Add(new AuditLog
        {
            Action = "AI Reply Draft Generated",
            Details = message.Email,
            Actor = User.Identity?.Name ?? "Admin"
        });
        await context.SaveChangesAsync();

        TempData["MessageStatus"] = "AI draft generated. Review and save the reply when ready.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reply(int id, string? reply)
    {
        var message = await context.ContactMessages.FindAsync(id);
        if (message is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(reply))
        {
            TempData["MessageStatus"] = "Reply cannot be empty.";
            return RedirectToAction(nameof(Index));
        }

        message.Reply = reply.Trim();
        message.RepliedAt = DateTime.UtcNow;
        message.RepliedBy = User.Identity?.Name ?? "Admin";
        message.Status = ContactMessageStatuses.Replied;
        message.IsRead = true;

        context.AuditLogs.Add(new AuditLog
        {
            Action = "Contact Message Replied",
            Details = message.Email,
            Actor = User.Identity?.Name ?? "Admin"
        });

        await context.SaveChangesAsync();
        TempData["MessageStatus"] = "Reply saved successfully.";
        return RedirectToAction(nameof(Index));
    }
}
