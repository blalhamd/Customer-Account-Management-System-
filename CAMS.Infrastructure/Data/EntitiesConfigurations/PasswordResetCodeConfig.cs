using CAMS.Domains.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CAMS.Infrastructure.Data.EntitiesConfigurations
{
    public partial class AccountConfiguration
    {
        public class PasswordResetCodeConfig : IEntityTypeConfiguration<PasswordResetCode>
        {
            public void Configure(EntityTypeBuilder<PasswordResetCode> builder)
            {
                builder.ToTable("PasswordResetCodes").HasKey(t => t.Id);
                builder.Property(x=> x.IPAddress).HasMaxLength(128);
                builder.Property(x=> x.UserId).HasMaxLength(128);
                builder.Property(x=> x.CodeHash).HasMaxLength(512);
            }
        }
    }
}
