using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Data.Configurations;

public class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.DeviceFingerprint)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(d => d.FriendlyName)
            .HasMaxLength(200);

        builder.Property(d => d.Platform)
            .HasMaxLength(200);

        builder.Property(d => d.DeviceType)
            .HasMaxLength(50);

        builder.Property(d => d.IpAddress)
            .HasMaxLength(45); // IPv6

        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Unique: one fingerprint per user
        builder.HasIndex(d => new { d.UserId, d.DeviceFingerprint })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasOne(d => d.User)
            .WithMany(u => u.Devices)
            .HasForeignKey(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Soft-delete global filter
        builder.HasQueryFilter(d => !d.IsDeleted);
    }
}
