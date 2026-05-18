using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Infrastructure.Data.Configurations
{
    internal class VerificationRequestConfiguration : IEntityTypeConfiguration<VerificationRequest>
    {
        public void Configure(EntityTypeBuilder<VerificationRequest> builder)
        {
            builder.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.RequiredDocument).WithMany(x => x.Requests).HasForeignKey(x => x.RequiredDocumentId);
        }
    }
}
