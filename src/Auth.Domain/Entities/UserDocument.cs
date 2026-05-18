using Auth.Domain.Common;
using Auth.Domain.Entities;

namespace Auth.Domain.Entities;

public class UserDocument : AuditableEntity
{
    public Guid VerificationRequestId { get; set; }
    public string FileUrl { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public string? FileType { get; set; }
    public string? MetadataJson { get; set; }

    /// <summary>Alias for AuditableEntity.CreatedAt — when the file was uploaded.</summary>
    public DateTime UploadedAt => CreatedAt;

    // Navigation
    public VerificationRequest VerificationRequest { get; set; } = null!;
}
