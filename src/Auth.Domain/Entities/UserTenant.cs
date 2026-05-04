namespace Auth.Domain.Entities;

public class UserTenant
{
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public bool IsDefault { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
