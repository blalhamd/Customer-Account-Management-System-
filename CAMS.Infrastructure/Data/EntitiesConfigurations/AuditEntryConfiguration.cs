using CAMS.Domains.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace CAMS.Infrastructure.Data.EntitiesConfigurations
{
    public class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
    {
        public void Configure(EntityTypeBuilder<AuditEntry> builder)
        {
            builder.ToTable("AuditEntries").HasKey(e => e.Id);
            builder.Property(x => x.Id)
                   .HasDefaultValueSql("NEWID()");

            builder.Property(x => x.TargetUserId).HasMaxLength(450);
            builder.Property(x => x.ActorUserId).HasMaxLength(450);
            builder.Property(x => x.IpAddress).HasMaxLength(256);
            builder.Property(x => x.EntityId).HasMaxLength(450);
            builder.Property(x => x.Action).HasMaxLength(256);
            builder.Property(x => x.EntityName).HasMaxLength(256);
            builder.Property(x => x.Summary).HasMaxLength(450);
        }
    }
}
