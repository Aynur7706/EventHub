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

        var categories = await context.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);
        var existingTitles = await context.Events.Select(e => e.Title).ToListAsync();
        var seedEvents = new[]
        {
            CreateEvent("Baku Startup Summit", "Founders, investors and product teams meet for a practical startup day with pitch sessions, networking and investor panels.", "Baku Convention Center", 49, 250, 18, "/images/events/startup.svg", categories["Business"], admin.Id),
            CreateEvent("AI Product Workshop", "Hands-on workshop about building useful AI features for web products, from prompt design to user experience and evaluation.", "ADA University", 29, 120, 25, "/images/events/ai.svg", categories["Technology"], organizer.Id),
            CreateEvent("Design Systems Evening", "A focused meetup for UI engineers and product designers covering tokens, reusable components and product consistency.", "Port Baku", 19, 90, 32, "/images/events/design.svg", categories["Education"], organizer.Id),
            CreateEvent("Caspian Tech Expo", "A full-day technology exhibition with local startups, software teams, AI demos and cloud engineering talks.", "Baku Expo Center", 35, 400, 12, "/images/events/tech-expo.svg", categories["Technology"], organizer.Id),
            CreateEvent("Digital Marketing Bootcamp", "Practical sessions on brand strategy, performance ads, analytics and content systems for modern businesses.", "Hilton Baku", 24, 160, 21, "/images/events/marketing.svg", categories["Business"], admin.Id),
            CreateEvent("Jazz Night by the Sea", "An elegant live music evening with local jazz performers, seaside ambience and a relaxed networking atmosphere.", "Baku Boulevard", 18, 220, 28, "/images/events/jazz.svg", categories["Music"], organizer.Id),
            CreateEvent("Startup Pitch Battle", "Early-stage founders compete in a live pitch format with mentor feedback, audience voting and sponsor prizes.", "SABAH.lab Innovation Center", 15, 180, 35, "/images/events/pitch.svg", categories["Business"], organizer.Id),
            CreateEvent("Frontend Masters Meetup", "A developer meetup about ASP.NET Core MVC frontends, responsive UI patterns and clean JavaScript interactions.", "Code Academy Baku", 12, 140, 16, "/images/events/frontend.svg", categories["Technology"], admin.Id),
            CreateEvent("Education Innovation Forum", "Teachers, mentors and edtech founders discuss digital learning, student engagement and future-ready skills.", "Azerbaijan State Economic University", 10, 300, 42, "/images/events/education.svg", categories["Education"], organizer.Id),
            CreateEvent("Baku Marathon Community Run", "A friendly city run for sport lovers with registration tracking, route support and community activities.", "National Boulevard", 8, 600, 30, "/images/events/marathon.svg", categories["Sport"], admin.Id),
            CreateEvent("Creative Business Brunch", "A morning event for creators and small business owners with talks on pricing, sales and personal branding.", "YARAT Contemporary Art Space", 22, 110, 24, "/images/events/brunch.svg", categories["Business"], organizer.Id),
            CreateEvent("Cybersecurity Awareness Day", "Security specialists explain account protection, phishing prevention and secure development practices.", "Baku Higher Oil School", 20, 200, 38, "/images/events/cybersecurity.svg", categories["Technology"], admin.Id)
        };

        context.Events.AddRange(seedEvents.Where(e => !existingTitles.Contains(e.Title)));
        await context.SaveChangesAsync();
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
