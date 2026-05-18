using Auth.Domain.Common;

namespace Auth.Domain.Entities;

public class Tenant : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Subdomain { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>Tenant-level device limit. Null = unlimited (falls back to global default).</summary>
    public int? MaxDevicesPerUser { get; set; }

    // Navigation properties
    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
    public ICollection<ApplicationRole> Roles { get; set; } = new List<ApplicationRole>();
}
