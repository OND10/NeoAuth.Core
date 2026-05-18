using Microsoft.AspNetCore.Http;

namespace Auth.Infrastructure.Helpers;

public static class FileHelper
{
    /// <summary>
    /// Helper to upload a file to a specific directory within the 'documents' folder.
    /// </summary>
    public static async Task<string> UploadToDocumentsAsync(IFormFile file, string rootPath, string specificDirectory)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty");

        // Path.Combine into documents folder as requested
        var documentsFolder = "documents";
        var targetPath = Path.Combine(rootPath, documentsFolder, specificDirectory);

        if (!Directory.Exists(targetPath))
            Directory.CreateDirectory(targetPath);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(targetPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return Path.Combine(documentsFolder, specificDirectory, fileName).Replace("\\", "/");
    }
}
