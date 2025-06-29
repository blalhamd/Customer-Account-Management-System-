using CAMS.Domains.Entities;
using CAMS.Domains.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore;

namespace CAMS.Infrastructure.Data.EntitiesConfigurations
{
    public class TransactionConfig : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("Transactions").HasKey(x => x.Id);
            builder.Property(x => x.Id)
                   .HasDefaultValueSql("NEWID()");

            builder.Property(x => x.TransactionType)
                            .HasConversion(new EnumToStringConverter<TransactionType>());

            builder.Property(x => x.AccountId).HasMaxLength(450).IsRequired();
            builder.Property(x => x.Amount).HasColumnType("decimal").HasPrecision(18, 2);
            builder.Property(x => x.Discount).HasColumnType("decimal").HasPrecision(18, 2);
        }
    }
}
