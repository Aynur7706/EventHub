namespace EventHub.Web.Interfaces;

public interface IImageStorageService
{
    Task<string> SaveAsync(IFormFile? file, string fallbackUrl);
}
