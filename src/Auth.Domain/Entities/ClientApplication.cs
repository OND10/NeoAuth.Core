using Auth.Domain.Common;

namespace Auth.Domain.Entities;

public class ClientApplication : AuditableEntity
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecretHash { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<ClientScope> AllowedScopes { get; set; } = new List<ClientScope>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
