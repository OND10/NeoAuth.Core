using Auth.Domain.Common;

namespace Auth.Domain.Entities;

/// <summary>
/// Optional per-role override for maximum device count.
/// If no entry exists for a role, falls back to Tenant.MaxDevicesPerUser → global default.
/// </summary>
public class RoleDeviceLimit : AuditableEntity
{
    public Guid RoleId { get; set; }
    public int MaxDevices { get; set; }

    // Navigation
    public ApplicationRole Role { get; set; } = null!;
}
