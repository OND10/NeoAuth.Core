namespace Auth.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
    public Guid? UserId { get; set; }
    public Guid? ClientApplicationId { get; set; }
    public Guid? UserDeviceId { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt is not null;
    public bool IsActive => !IsRevoked && !IsExpired;

    // Navigation properties
    public ApplicationUser? User { get; set; }
    public ClientApplication? ClientApplication { get; set; }
    public UserDevice? Device { get; set; }
}
