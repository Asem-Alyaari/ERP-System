using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class JournalEntryMasterConfiguration : IEntityTypeConfiguration<JournalEntryMaster>
{
    public void Configure(EntityTypeBuilder<JournalEntryMaster> builder)
    {
        builder.ToTable("JournalEntryMasters");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.VoucherNumber)
            .IsRequired()
            .HasMaxLength(50);

        // Unique index for VoucherNumber
        builder.HasIndex(x => x.VoucherNumber)
            .IsUnique();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PostedBy)
            .HasMaxLength(100);

        // Relationship with FiscalPeriod
        builder.HasOne(x => x.FiscalPeriod)
            .WithMany()
            .HasForeignKey(x => x.FiscalPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        // One-to-many relationship with Lines
        builder.HasMany(x => x.Lines)
            .WithOne(x => x.JournalEntryMaster)
            .HasForeignKey(x => x.JournalEntryMasterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
