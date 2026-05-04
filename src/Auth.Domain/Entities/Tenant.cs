namespace Auth.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Subdomain { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
    public ICollection<ApplicationRole> Roles { get; set; } = new List<ApplicationRole>();
}
