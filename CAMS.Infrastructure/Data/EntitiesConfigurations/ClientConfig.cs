using CAMS.Domains.Entities;
using CAMS.Domains.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;

namespace CAMS.Infrastructure.Data.EntitiesConfigurations
{
    public class ClientConfig : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("Clients").HasKey(t => t.Id);
            builder.Property(x => x.Id)
                   .HasDefaultValueSql("NEWID()");

            builder.OwnsOne(x => x.Address);

            builder.Property(a => a.FullName).HasMaxLength(256).IsRequired();
            builder.Property(a => a.SSN).HasMaxLength(256).IsRequired();
            builder.Property(a => a.ImagePath).HasMaxLength(512).IsRequired();
            builder.Property(a => a.Nationality).HasMaxLength(512).IsRequired();
            builder.Property(a => a.JobTitle).HasMaxLength(512).IsRequired();

            builder.Property(x => x.Gender)
                   .HasConversion(new EnumToStringConverter<Gender>());

            builder.Property(x => x.MonthlyIncome).HasColumnType("decimal").HasPrecision(18, 2);
            builder.Property(x => x.FinancialSource).HasColumnType("decimal").HasPrecision(18, 2);
        }
    }
}
