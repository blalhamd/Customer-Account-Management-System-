using CAMS.Domains.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace CAMS.Infrastructure.Data.EntitiesConfigurations
{
    public class CurrentConfig : IEntityTypeConfiguration<Current>
    {
        public void Configure(EntityTypeBuilder<Current> builder)
        {
            builder.Property(t => t.MaximumWithdrawal).HasColumnType("decimal").HasPrecision(18, 2);
            builder.Property(t => t.MinimumBalance).HasColumnType("decimal").HasPrecision(18, 2);
            builder.Property(t => t.MonthlyFee).HasColumnType("decimal").HasPrecision(18, 2);
        }
    }
}
