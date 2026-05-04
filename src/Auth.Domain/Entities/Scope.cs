namespace Auth.Domain.Entities;

public class Scope
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Hierarchical support
    public Guid? ParentId { get; set; }
    public Scope? Parent { get; set; }
    public ICollection<Scope> Children { get; set; } = new List<Scope>();

    public ICollection<ClientScope> ClientScopes { get; set; } = new List<ClientScope>();
}
