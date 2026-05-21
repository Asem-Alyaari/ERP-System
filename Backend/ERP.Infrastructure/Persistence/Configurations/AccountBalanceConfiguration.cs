using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class AccountBalanceConfiguration : IEntityTypeConfiguration<AccountBalance>
{
    public void Configure(EntityTypeBuilder<AccountBalance> builder)
    {
        builder.ToTable("AccountBalances");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TotalDebit)
            .HasPrecision(18, 4);

        builder.Property(x => x.TotalCredit)
            .HasPrecision(18, 4);

        builder.Property(x => x.CurrentBalance)
            .HasPrecision(18, 4);

        // المركب الفهرس الفريد (Unique Composite Index)
        builder.HasIndex(x => new { x.FiscalPeriodId, x.AccountId, x.CostCenterId, x.CurrencyId })
            .IsUnique();

        // العلاقات
        builder.HasOne(x => x.FiscalPeriod)
            .WithMany()
            .HasForeignKey(x => x.FiscalPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CostCenter)
            .WithMany()
            .HasForeignKey(x => x.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
