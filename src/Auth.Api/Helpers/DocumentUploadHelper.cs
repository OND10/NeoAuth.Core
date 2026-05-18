using Microsoft.AspNetCore.Http;

namespace Auth.Api.Helpers;

/// <summary>
/// Handles uploading IFormFile collections to the Documents folder.
/// Returns relative URLs suitable for storing in the database.
/// </summary>
public static class DocumentUploadHelper
{
    private const string DocumentsFolder = "Documents";

    /// <summary>
    /// Processes and saves multiple files for a single document submission.
    /// Files are stored at: wwwroot/Documents/{userId}/{requiredDocumentId}/{guid}{ext}
    /// </summary>
    /// <param name="files">The uploaded files.</param>
    /// <param name="webRootPath">The application's WebRootPath (wwwroot).</param>
    /// <param name="userId">The ID of the user submitting the document.</param>
    /// <param name="requiredDocumentId">The ID of the required document definition.</param>
    /// <param name="minFilesRequired">Minimum number of files required; throws if not met.</param>
    /// <returns>A list of relative URL strings for each saved file.</returns>
    public static async Task<IReadOnlyList<string>> SaveDocumentFilesAsync(
        IEnumerable<IFormFile> files,
        string webRootPath,
        Guid userId,
        Guid requiredDocumentId,
        int minFilesRequired)
    {
        var fileList = files?.ToList() ?? [];

        if (fileList.Count < minFilesRequired)
            throw new InvalidOperationException(
                $"At least {minFilesRequired} file(s) are required for this document, but {fileList.Count} were provided.");

        // Build the target directory using Path.Combine into the Documents folder
        var targetDirectory = Path.Combine(
            webRootPath,
            DocumentsFolder,
            userId.ToString(),
            requiredDocumentId.ToString());

        Directory.CreateDirectory(targetDirectory);

        var savedUrls = new List<string>(fileList.Count);

        foreach (var file in fileList)
        {
            if (file.Length == 0) continue;

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(targetDirectory, uniqueFileName);

            await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            await file.CopyToAsync(stream);

            // Return a forward-slash relative URL for web access
            var relativeUrl = $"/{DocumentsFolder}/{userId}/{requiredDocumentId}/{uniqueFileName}";
            savedUrls.Add(relativeUrl);
        }

        return savedUrls.AsReadOnly();
    }

    /// <summary>
    /// Processes all items in a bulk submission.
    /// Returns a mapping of RequiredDocumentId → list of saved file URLs.
    /// </summary>
    public static async Task<Dictionary<Guid, IReadOnlyList<string>>> SaveBulkDocumentFilesAsync(
        IEnumerable<BulkDocumentInput> inputs,
        string webRootPath,
        Guid userId)
    {
        var result = new Dictionary<Guid, IReadOnlyList<string>>();

        foreach (var input in inputs)
        {
            var urls = await SaveDocumentFilesAsync(
                input.Files,
                webRootPath,
                userId,
                input.RequiredDocumentId,
                input.MinFilesRequired);

            result[input.RequiredDocumentId] = urls;
        }

        return result;
    }
}

/// <summary>
/// Input model used by DocumentUploadHelper for bulk processing.
/// </summary>
public record BulkDocumentInput(
    Guid RequiredDocumentId,
    IEnumerable<IFormFile> Files,
    int MinFilesRequired);
