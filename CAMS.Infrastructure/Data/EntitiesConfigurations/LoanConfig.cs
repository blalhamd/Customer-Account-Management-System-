using CAMS.Domains.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace CAMS.Infrastructure.Data.EntitiesConfigurations
{
    public class LoanConfig : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            builder.Property(x => x.LoanAmount).HasColumnType("decimal").HasPrecision(18, 2);
            builder.Property(x => x.InterestRate).HasColumnType("decimal").HasPrecision(18, 2);
            builder.Property(x => x.MonthlyInstallment).HasColumnType("float");
            builder.Property(x => x.RemainingBalance).HasColumnType("decimal").HasPrecision(18, 2);
        }
    }
}
