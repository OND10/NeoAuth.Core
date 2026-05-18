using Auth.Domain.Common;
using Auth.Domain.Enums;

namespace Auth.Domain.Entities;

/// <summary>
/// Represents a registered device for a user.
/// A user can only have a limited number of active devices at once.
/// </summary>
public class UserDevice : AuditableEntity
{
    public Guid UserId { get; set; }

    // ── Identification ──────────────────────────────
    /// <summary>
    /// SHA-256 hash of the client-generated device fingerprint.
    /// Never store the raw fingerprint.
    /// </summary>
    public string DeviceFingerprint { get; set; } = string.Empty;

    // ── Metadata ────────────────────────────────────
    public string FriendlyName { get; set; } = string.Empty;
    public string? Platform { get; set; }
    public string? DeviceType { get; set; }   // "Mobile" | "Browser" | "Desktop"
    public string? IpAddress { get; set; }

    // ── Lifecycle ───────────────────────────────────
    public DateTime LastActivityAt { get; set; } = DateTime.UtcNow;
    public DeviceStatus Status { get; set; } = DeviceStatus.Active;
    public DateTime? RevokedAt { get; set; }

    // ── Navigation ──────────────────────────────────
    public ApplicationUser User { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
