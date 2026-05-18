using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Infrastructure.Data.Configurations;

public class RoleDeviceLimitConfiguration : IEntityTypeConfiguration<RoleDeviceLimit>
{
    public void Configure(EntityTypeBuilder<RoleDeviceLimit> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.RoleId).IsUnique();

        builder.HasOne(r => r.Role)
            .WithMany()
            .HasForeignKey(r => r.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
