using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Auth.Infrastructure.Services;

public class LocalFileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly string _uploadFolder = "uploads";

    public LocalFileService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        var contentPath = _environment.WebRootPath;
        if (string.IsNullOrEmpty(contentPath))
        {
            // Fallback for environments without WebRootPath set
            contentPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var path = Path.Combine(contentPath, _uploadFolder, subFolder);

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(path, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Return relative path for URL generation
        return $"/{_uploadFolder}/{subFolder}/{fileName}";
    }

    public Task DeleteFileAsync(string fileUrl)
    {
        // Implementation for deletion if needed
        return Task.CompletedTask;
    }

    public async Task<string> UploadToDocumentsAsync(IFormFile file, string specificDirectory)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        var contentPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        
        // Path.Combine into documents folder as requested
        var documentsFolder = "documents";
        var targetPath = Path.Combine(contentPath, documentsFolder, specificDirectory);

        if (!Directory.Exists(targetPath))
            Directory.CreateDirectory(targetPath);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(targetPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"/{documentsFolder}/{specificDirectory}/{fileName}";
    }
}
