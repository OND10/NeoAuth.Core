using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Data.Configurations;

public class ClientScopeConfiguration : IEntityTypeConfiguration<ClientScope>
{
    public void Configure(EntityTypeBuilder<ClientScope> builder)
    {
        builder.HasKey(cs => new { cs.ClientApplicationId, cs.ScopeId });

        builder.HasOne(cs => cs.ClientApplication)
              .WithMany(c => c.AllowedScopes)
              .HasForeignKey(cs => cs.ClientApplicationId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.Scope)
              .WithMany(s => s.ClientScopes)
              .HasForeignKey(cs => cs.ScopeId)
              .OnDelete(DeleteBehavior.Cascade);
    }
}
