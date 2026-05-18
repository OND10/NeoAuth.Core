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
    public class UserDocumentConfiguration : IEntityTypeConfiguration<UserDocument>
    {
        public void Configure(EntityTypeBuilder<UserDocument> builder)
        {
            builder.HasOne(x => x.VerificationRequest).WithMany(x => x.Documents).HasForeignKey(x => x.VerificationRequestId);

        }
    }
}
