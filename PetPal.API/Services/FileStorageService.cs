using Microsoft.AspNetCore.Http;

namespace PetPal.API.Services;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string containerName);
    Task DeleteFileAsync(string filePath, string containerName);
    string GetFileUrl(string fileName, string containerName);
}

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FileStorageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string containerName)
    {
        // Validate file
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null", nameof(file));
        }

        // Get allowed extensions based on container name
        string[] allowedExtensions;
        if (containerName.Contains("documents"))
        {
            // Allow document file types
            allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".jpg", ".jpeg", ".png" };
        }
        else
        {
            // Default to only images for other containers
            allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        }

        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new ArgumentException($"File type {fileExtension} is not allowed. Allowed types: {string.Join(", ", allowedExtensions)}");
        }

        // Create directory if it doesn't exist
        var uploadDir = Path.Combine(_env.WebRootPath, containerName);
        if (!Directory.Exists(uploadDir))
        {
            Directory.CreateDirectory(uploadDir);
        }

        // Generate unique filename
        var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadDir, uniqueFileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return uniqueFileName;
    }

    public Task DeleteFileAsync(string fileName, string containerName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return Task.CompletedTask;
        }

        var filePath = Path.Combine(_env.WebRootPath, containerName, fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    public string GetFileUrl(string fileName, string containerName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        var request = _httpContextAccessor.HttpContext.Request;
        return $"{request.Scheme}://{request.Host}/{containerName}/{fileName}";
    }
}
