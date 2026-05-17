using EventHub.Web.Constants;
using EventHub.Web.Data;
using EventHub.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace EventHub.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppRoles.Admin)]
public class RegistrationsController(ApplicationDbContext context) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = await context.Registrations
            .Include(r => r.User)
            .Include(r => r.Event)
            .OrderByDescending(r => r.RegisteredAt)
            .Select(r => new AdminRegistrationViewModel
            {
                Id = r.Id,
                CustomerName = r.User!.FullName,
                CustomerEmail = r.User.Email ?? string.Empty,
                EventTitle = r.Event!.Title,
                EventDate = r.Event.EventDate,
                TicketCount = r.TicketCount,
                TotalPrice = r.TotalPrice,
                TicketCode = r.TicketCode,
                Status = r.Status,
                CheckedInAt = r.CheckedInAt,
                RegisteredAt = r.RegisteredAt
            })
            .ToListAsync();

        return View(model);
    }

    public async Task<IActionResult> ExportCsv()
    {
        var registrations = await context.Registrations
            .Include(r => r.User)
            .Include(r => r.Event)
            .OrderByDescending(r => r.RegisteredAt)
            .ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Customer,Email,Event,EventDate,TicketCode,Status,TicketCount,TotalPrice");
        foreach (var item in registrations)
        {
            csv.AppendLine(string.Join(",", new[]
            {
                Escape(item.User?.FullName),
                Escape(item.User?.Email),
                Escape(item.Event?.Title),
                Escape(item.Event?.EventDate.ToString("yyyy-MM-dd HH:mm")),
                Escape(item.TicketCode),
                Escape(item.Status),
                item.TicketCount.ToString(),
                item.TotalPrice.ToString("0.00")
            }));
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", "eventhub-tickets.csv");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var registration = await context.Registrations.FindAsync(id);
        if (registration is not null)
        {
            registration.Status = RegistrationStatuses.Cancelled;
            await context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    public IActionResult CheckIn() => View(new AdminCheckInViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CheckIn(AdminCheckInViewModel model)
    {
        var code = model.TicketCode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            model.Message = "Please enter a ticket code.";
            model.TicketCode = string.Empty;
            return View(model);
        }

        var registration = await context.Registrations
            .Include(r => r.User)
            .Include(r => r.Event)
            .FirstOrDefaultAsync(r => r.TicketCode == code);

        if (registration is null)
        {
            model.Message = "Ticket code was not found.";
            return View(model);
        }

        if (registration.Status == RegistrationStatuses.Cancelled)
        {
            model.Message = "This ticket is cancelled and cannot be checked in.";
        }
        else if (registration.Status == RegistrationStatuses.CheckedIn)
        {
            model.Message = "This ticket was already checked in.";
        }
        else
        {
            registration.Status = RegistrationStatuses.CheckedIn;
            registration.CheckedInAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
            model.IsSuccess = true;
            model.Message = "Ticket checked in successfully.";
        }

        model.Ticket = ToAdminRegistrationViewModel(registration);
        return View(model);
    }

    private static AdminRegistrationViewModel ToAdminRegistrationViewModel(EventHub.Web.Models.Registration registration) => new()
    {
        Id = registration.Id,
        CustomerName = registration.User?.FullName ?? string.Empty,
        CustomerEmail = registration.User?.Email ?? string.Empty,
        EventTitle = registration.Event?.Title ?? string.Empty,
        EventDate = registration.Event?.EventDate ?? DateTime.MinValue,
        TicketCount = registration.TicketCount,
        TotalPrice = registration.TotalPrice,
        TicketCode = registration.TicketCode,
        Status = registration.Status,
        CheckedInAt = registration.CheckedInAt,
        RegisteredAt = registration.RegisteredAt
    };

    private static string Escape(string? value) => $"\"{(value ?? string.Empty).Replace("\"", "\"\"")}\"";
}
