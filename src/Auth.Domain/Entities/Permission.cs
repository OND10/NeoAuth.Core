using Auth.Domain.Common;

namespace Auth.Domain.Entities;

public class Permission : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Hierarchical groups
    public Guid? ParentId { get; set; }
    public Permission? Parent { get; set; }
    public ICollection<Permission> Children { get; set; } = new List<Permission>();

    // Navigation properties
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
