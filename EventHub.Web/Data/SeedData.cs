using EventHub.Web.Constants;
using EventHub.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await context.Database.EnsureCreatedAsync();

        foreach (var role in new[] { AppRoles.Admin, AppRoles.Organizer, AppRoles.User })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        var admin = await CreateUserAsync(userManager, "admin@eventhub.local", "Admin User", AppRoles.Admin);
        var organizer = await CreateUserAsync(userManager, "organizer@eventhub.local", "NexEvent Organizer", AppRoles.Organizer);
        await CreateUserAsync(userManager, "user@eventhub.local", "Demo User", AppRoles.User);

        if (!await context.Categories.AnyAsync())
        {
            context.Categories.AddRange(
                new Category { Name = "Technology" },
                new Category { Name = "Business" },
                new Category { Name = "Music" },
                new Category { Name = "Sport" },
                new Category { Name = "Education" });
            await context.SaveChangesAsync();
        }

        if (!await context.Events.AnyAsync())
        {
            var categories = await context.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);
            context.Events.AddRange(
                CreateEvent("Baku Startup Summit", "Founders, investors and product teams meet for a practical startup day.", "Baku Convention Center", 49, 250, 18, "/images/events/startup.svg", categories["Business"], admin.Id),
                CreateEvent("AI Product Workshop", "Hands-on workshop about building useful AI features for web products.", "ADA University", 29, 120, 25, "/images/events/ai.svg", categories["Technology"], organizer.Id),
                CreateEvent("Design Systems Evening", "A focused meetup for UI engineers and product designers.", "Port Baku", 19, 90, 32, "/images/events/design.svg", categories["Education"], organizer.Id));
            await context.SaveChangesAsync();
        }
    }

    private static async Task<ApplicationUser> CreateUserAsync(UserManager<ApplicationUser> userManager, string email, string fullName, string role)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(user, "EventHub123!");
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }

        return user;
    }

    private static Event CreateEvent(string title, string description, string location, decimal price, int capacity, int daysFromNow, string imageUrl, int categoryId, string organizerId) => new()
    {
        Title = title,
        Description = description,
        Location = location,
        Price = price,
        Capacity = capacity,
        EventDate = DateTime.Today.AddDays(daysFromNow).AddHours(18),
        ImageUrl = imageUrl,
        CategoryId = categoryId,
        OrganizerId = organizerId
    };
}
