namespace Auth.Domain.Entities;

public class UserPermission
{
    public Guid UserId { get; set; }
    public Guid PermissionId { get; set; }

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
