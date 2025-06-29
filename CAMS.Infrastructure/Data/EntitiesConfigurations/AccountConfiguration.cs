using CAMS.Domains.Entities;
using CAMS.Domains.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;

namespace CAMS.Infrastructure.Data.EntitiesConfigurations
{
    public partial class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Accounts").HasKey(e => e.Id);

            builder.Property(x => x.Id)
                   .HasDefaultValueSql("NEWID()");

            builder.HasDiscriminator<string>("Type");

            builder.HasIndex(a => a.AccountNumber).IsUnique();

            builder.Property(a => a.AccountNumber).HasMaxLength(256).IsRequired();
            builder.Property(a => a.ClientId).HasMaxLength(450).IsRequired();
            builder.Property(a => a.Branch).HasMaxLength(256).IsRequired();

            builder.Property(x => x.Balance).HasColumnType("decimal").HasPrecision(18, 2);

            builder.Property(a => a.CurrencyType)
                   .HasConversion(new EnumToStringConverter<CurrencyType>());

            builder.Property(a => a.AccountStatus)
                   .HasConversion(new EnumToStringConverter<AccountStatus>());
        }
    }
}
