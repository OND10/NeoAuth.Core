using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(p => p.Name).IsUnique();
        builder.Property(p => p.Description).HasMaxLength(500);

        builder.HasOne(p => p.Parent)
              .WithMany(p => p.Children)
              .HasForeignKey(p => p.ParentId)
              .OnDelete(DeleteBehavior.Restrict);
    }
}
