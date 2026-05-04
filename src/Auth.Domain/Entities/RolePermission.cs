namespace Auth.Domain.Entities;

public class RolePermission
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }

    // Navigation properties
    public ApplicationRole Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
