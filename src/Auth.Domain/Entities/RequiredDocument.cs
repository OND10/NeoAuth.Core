using Auth.Domain.Common;
using Auth.Domain.Entities;

namespace Auth.Domain.Entities;

public class RequiredDocument : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ValidationRulesJson { get; set; }
    public int MinFilesRequired { get; set; } = 1;

    public bool RequiresDocumentNumber { get; set; }
    public bool RequiresIssueDate { get; set; }
    public bool RequiresExpiryDate { get; set; }

    // JSON schema for additional dynamic fields if needed
    public string? MetadataSchemaJson { get; set; }
    public Guid? TenantId { get; set; }
    public bool IsActive { get; set; } = true;

    // The role that requires this document (if null, it's for everyone or handled manually)
    public Guid? TriggerRoleId { get; set; }

    // The role that this verification leads to (Post-Verification Role)
    public Guid TargetRoleId { get; set; }

    // Navigation
    public Tenant? Tenant { get; set; }
    public ApplicationRole? TriggerRole { get; set; }
    public ApplicationRole TargetRole { get; set; } = null!;
    public ICollection<VerificationRequest> Requests { get; set; } = new List<VerificationRequest>();
}
