using Auth.Domain.Common;

namespace Auth.Domain.Entities;

public class Scope : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Hierarchical support
    public Guid? ParentId { get; set; }
    public Scope? Parent { get; set; }
    public ICollection<Scope> Children { get; set; } = new List<Scope>();

    public ICollection<ClientScope> ClientScopes { get; set; } = new List<ClientScope>();
}
