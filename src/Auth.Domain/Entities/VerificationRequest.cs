using Auth.Domain.Common;
using Auth.Domain.Entities;
using Auth.Domain.Enums;

namespace Auth.Domain.Entities;

public class VerificationRequest : AuditableEntity
{
    public Guid UserId { get; set; }
    public Guid? TenantId { get; set; }
    public Guid RequiredDocumentId { get; set; }
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    public string? AdminNotes { get; set; }

    // Document Metadata
    public string? DocumentNumber { get; set; }
    public DateTime? IssuedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // Other dynamic metadata stored as JSON
    public string? MetadataJson { get; set; }

    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedBy { get; set; }

    // Navigation
    public ApplicationUser User { get; set; } = null!;
    public Tenant? Tenant { get; set; }
    public RequiredDocument RequiredDocument { get; set; } = null!;
    public ICollection<UserDocument> Documents { get; set; } = new List<UserDocument>();
}
