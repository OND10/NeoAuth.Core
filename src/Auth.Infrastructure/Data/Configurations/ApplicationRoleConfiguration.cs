using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Data.Configurations;

public class ApplicationRoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.Property(r => r.Description).HasMaxLength(500);
        builder.HasOne(r => r.Tenant)
              .WithMany(t => t.Roles)
              .HasForeignKey(r => r.TenantId)
              .OnDelete(DeleteBehavior.SetNull);
    }
}
