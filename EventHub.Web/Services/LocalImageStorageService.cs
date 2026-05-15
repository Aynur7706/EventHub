using EventHub.Web.Interfaces;

namespace EventHub.Web.Services;

public class LocalImageStorageService(IWebHostEnvironment environment) : IImageStorageService
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    public async Task<string> SaveAsync(IFormFile? file, string fallbackUrl)
    {
        if (file is null || file.Length == 0)
        {
            return fallbackUrl;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return fallbackUrl;
        }

        var uploadRoot = Path.Combine(environment.WebRootPath, "uploads", "events");
        Directory.CreateDirectory(uploadRoot);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploadRoot, fileName);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream);

        return $"/uploads/events/{fileName}";
    }
}
