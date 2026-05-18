using Microsoft.AspNetCore.Http;

namespace Auth.Application.Interfaces;

public interface IFileService
{
    /// <summary>
    /// Saves a file and returns its public URL or path.
    /// </summary>
    Task<string> SaveFileAsync(IFormFile file, string subFolder);
    
    /// <summary>
    /// Deletes a file.
    /// </summary>
    Task DeleteFileAsync(string fileUrl);

    /// <summary>
    /// Uploads a file to a specific directory within the 'documents' folder.
    /// </summary>
    Task<string> UploadToDocumentsAsync(IFormFile file, string specificDirectory);
}
