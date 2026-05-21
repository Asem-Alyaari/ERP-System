using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("JournalEntryLines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Debit)
            .HasPrecision(18, 4);

        builder.Property(x => x.Credit)
            .HasPrecision(18, 4);

        builder.Property(x => x.ForeignDebit)
            .HasPrecision(18, 4);

        builder.Property(x => x.ForeignCredit)
            .HasPrecision(18, 4);

        builder.Property(x => x.ExchangeRate)
            .HasPrecision(18, 6);

        builder.Property(x => x.Memo)
            .HasMaxLength(500);

        // Relationship with Account
        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with Currency
        builder.HasOne(x => x.Currency)
            .WithMany()
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship with CostCenter
        builder.HasOne(x => x.CostCenter)
            .WithMany(x => x.JournalEntryLines)
            .HasForeignKey(x => x.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
