using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
{
    public void Configure(EntityTypeBuilder<Currency> builder)
    {
        builder.ToTable("Currencies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Symbol)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.IsLocal)
            .IsRequired()
            .HasDefaultValue(false);

        // One-to-many relationship with CurrencyExchangeRate
        builder.HasMany(x => x.ExchangeRates)
            .WithOne(x => x.Currency)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Cascade);

        // One-to-many relationship with Accounts
        builder.HasMany(x => x.Accounts)
            .WithOne(x => x.Currency)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
