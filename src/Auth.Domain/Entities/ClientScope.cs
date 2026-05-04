namespace Auth.Domain.Entities;

public class ClientScope
{
    public Guid ClientApplicationId { get; set; }
    public ClientApplication ClientApplication { get; set; } = null!;

    public Guid ScopeId { get; set; }
    public Scope Scope { get; set; } = null!;
}
