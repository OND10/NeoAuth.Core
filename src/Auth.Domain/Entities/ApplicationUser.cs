using Auth.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace Auth.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public AuthProvider AuthProvider { get; set; } = AuthProvider.Local;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    /// <summary>
	/// Gets or sets the Google OAuth subject identifier
	/// </summary>
	public string? GoogleId { get; set; }

    // Navigation properties
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<UserTenant> UserTenants { get; set; } = new List<UserTenant>();
    public ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
}
