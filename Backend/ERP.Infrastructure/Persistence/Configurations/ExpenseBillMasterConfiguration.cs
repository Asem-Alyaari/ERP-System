using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class ExpenseBillMasterConfiguration : IEntityTypeConfiguration<ExpenseBillMaster>
{
    public void Configure(EntityTypeBuilder<ExpenseBillMaster> builder)
    {
        builder.ToTable("ExpenseBillMasters");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BillNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.BillNumber)
            .IsUnique();

        builder.Property(x => x.SupplierName)
            .HasMaxLength(200);

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.TaxAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.NetAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PostedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(x => x.Vendor)
            .WithMany()
            .HasForeignKey(x => x.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.PaymentAccount)
            .WithMany()
            .HasForeignKey(x => x.PaymentAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.ExpenseBillMaster)
            .HasForeignKey(x => x.ExpenseBillMasterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
