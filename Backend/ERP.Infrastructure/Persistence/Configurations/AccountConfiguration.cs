using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AccountCode)
            .IsRequired()
            .HasMaxLength(50);

        // Unique Index for AccountCode
        builder.HasIndex(x => x.AccountCode)
            .IsUnique();

        builder.Property(x => x.AccountNameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.AccountNameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.AccountType)
            .IsRequired();

        builder.Property(x => x.IsDetail)
            .IsRequired()
            .HasDefaultValue(true);

        // Self-referencing relationship (Parent-Child)
        builder.HasOne(x => x.ParentAccount)
            .WithMany(x => x.SubAccounts)
            .HasForeignKey(x => x.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent accidental deletion of children

        // Relationship with Currency
        builder.HasOne(x => x.Currency)
            .WithMany(x => x.Accounts)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
