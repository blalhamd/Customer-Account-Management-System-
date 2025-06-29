using CAMS.Domains.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace CAMS.Infrastructure.Data.EntitiesConfigurations
{
    public class FixedDepositConfig : IEntityTypeConfiguration<FixedDeposit>
    {
        public void Configure(EntityTypeBuilder<FixedDeposit> builder)
        {
            builder.Property(x => x.InterestEarned).HasColumnType("decimal").HasPrecision(18, 2);
            builder.Property(x => x.DepositAmount).HasColumnType("decimal").HasPrecision(18, 2);
        }
    }
}
