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
    internal class RequiredDocumentConfiguration : IEntityTypeConfiguration<RequiredDocument>
    {
        public void Configure(EntityTypeBuilder<RequiredDocument> builder)
        {
            builder.HasOne(x => x.Tenant).WithMany().HasForeignKey(x => x.TenantId);
            builder.HasOne(x => x.TargetRole).WithMany().HasForeignKey(x => x.TargetRoleId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}
