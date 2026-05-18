using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Token).HasMaxLength(500).IsRequired();
        builder.HasIndex(rt => rt.Token).IsUnique();
        builder.Property(rt => rt.ReplacedByToken).HasMaxLength(500);

        builder.HasOne(rt => rt.User)
              .WithMany(u => u.RefreshTokens)
              .HasForeignKey(rt => rt.UserId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rt => rt.ClientApplication)
              .WithMany(c => c.RefreshTokens)
              .HasForeignKey(rt => rt.ClientApplicationId)
              .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rt => rt.Device)
              .WithMany(d => d.RefreshTokens)
              .HasForeignKey(rt => rt.UserDeviceId)
              .OnDelete(DeleteBehavior.NoAction);
    }
}
