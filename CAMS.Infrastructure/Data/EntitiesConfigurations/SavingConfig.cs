using CAMS.Domains.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace CAMS.Infrastructure.Data.EntitiesConfigurations
{
    public class SavingConfig : IEntityTypeConfiguration<Saving>
    {
        public void Configure(EntityTypeBuilder<Saving> builder)
        {
            builder.Property(x => x.InterestRate).HasColumnType("decimal").HasPrecision(18, 2);
        }
    }
}
