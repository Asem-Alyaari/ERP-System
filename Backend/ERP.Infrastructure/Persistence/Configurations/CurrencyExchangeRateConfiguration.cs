using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class CurrencyExchangeRateConfiguration : IEntityTypeConfiguration<CurrencyExchangeRate>
{
    public void Configure(EntityTypeBuilder<CurrencyExchangeRate> builder)
    {
        builder.ToTable("CurrencyExchangeRates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Rate)
            .IsRequired()
            .HasPrecision(18, 6);

        builder.Property(x => x.EffectiveDate)
            .IsRequired();
    }
}
