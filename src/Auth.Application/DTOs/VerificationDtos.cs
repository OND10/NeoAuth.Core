using Auth.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Auth.Application.DTOs;

public record RequiredDocumentResponse(
    Guid Id,
    string Name,
    string? Description,
    int MinFilesRequired,
    Guid TargetRoleId
);

public record VerificationRequestDto(
    Guid Id,
    Guid UserId,
    Guid RequiredDocumentId,
    VerificationStatus Status,
    string? AdminNotes,
    DateTime CreatedAt,
    List<UserDocumentDto> Documents,
    string? DocumentNumber = null,
    DateTime? IssuedAt = null,
    DateTime? ExpiresAt = null,
    string? MetadataJson = null
);

public record UserDocumentDto(
    Guid Id,
    string FileUrl,
    string? FileName,
    DateTime UploadedAt
);

public record CreateVerificationRequest(
    Guid RequiredDocumentId,
    IFormFileCollection Files,
    string? DocumentNumber = null,
    DateTime? IssuedAt = null,
    DateTime? ExpiresAt = null,
    string? MetadataJson = null
);

public record BulkVerificationRequest(
    List<DocumentUploadItem> Documents
);

public record DocumentUploadItem(
    Guid RequiredDocumentId,
    IFormFileCollection Files,
    string? DocumentNumber = null,
    DateTime? IssuedAt = null,
    DateTime? ExpiresAt = null,
    string? MetadataJson = null
);

public record ReviewVerificationRequest(
    VerificationStatus Status,
    string? AdminNotes
);

// ─── Internal URL-based DTOs (used after API layer saves the files) ───────────

/// <summary>
/// Passed to DocumentService after files have already been saved by DocumentUploadHelper.
/// Contains URLs instead of IFormFile so the service layer stays infrastructure-free.
/// </summary>
public record ProcessedDocumentItem(
    Guid RequiredDocumentId,
    IReadOnlyList<string> FileUrls,
    string? DocumentNumber = null,
    DateTime? IssuedAt = null,
    DateTime? ExpiresAt = null,
    string? MetadataJson = null
);

public record BulkProcessedVerificationRequest(
    List<ProcessedDocumentItem> Documents
);

public record ConfigureRequiredDocumentRequest(
    string Name,
    string? Description,
    int MinFilesRequired,
    Guid TargetRoleId,
    Guid? TriggerRoleId = null,
    bool IsActive = true,
    bool RequiresDocumentNumber = false,
    bool RequiresIssueDate = false,
    bool RequiresExpiryDate = false,
    string? MetadataSchemaJson = null,
    string? ValidationRulesJson = null
);

public record RequiredDocumentDto(
    Guid Id,
    string Name,
    string? Description,
    int MinFilesRequired,
    Guid TargetRoleId,
    Guid? TriggerRoleId,
    bool IsActive,
    bool RequiresDocumentNumber = false,
    bool RequiresIssueDate = false,
    bool RequiresExpiryDate = false,
    string? MetadataSchemaJson = null,
    string? ValidationRulesJson = null
);

public class DocumentValidationRules
{
    public int? DocumentNumberMinLength { get; set; }
    public int? DocumentNumberMaxLength { get; set; }
    public string? DocumentNumberRegex { get; set; }
    public int? MinDurationDays { get; set; }
    public int? MaxDurationDays { get; set; }
    public int? MinDurationYears { get; set; }
    public int? MaxDurationYears { get; set; }
}
