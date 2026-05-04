using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Data.Configurations;

public class ClientApplicationConfiguration : IEntityTypeConfiguration<ClientApplication>
{
    public void Configure(EntityTypeBuilder<ClientApplication> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.ClientId).HasMaxLength(100).IsRequired();
        builder.HasIndex(c => c.ClientId).IsUnique();
        builder.Property(c => c.ClientSecretHash).HasMaxLength(500).IsRequired();
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Description).HasMaxLength(500);
    }
}
