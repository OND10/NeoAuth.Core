using Auth.Domain.Common;

namespace Auth.Domain.Entities;

public class Product : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
