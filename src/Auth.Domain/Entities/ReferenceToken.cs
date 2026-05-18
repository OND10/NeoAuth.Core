using Auth.Domain.Entities;

namespace Auth.Domain.Entities;

public class ReferenceToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = string.Empty;
    public Guid ClientId { get; set; }
    public string ClaimsJson { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ClientApplication Client { get; set; } = null!;
}
