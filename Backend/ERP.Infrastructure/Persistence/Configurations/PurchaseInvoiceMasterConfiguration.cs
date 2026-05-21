using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class PurchaseInvoiceMasterConfiguration : IEntityTypeConfiguration<PurchaseInvoiceMaster>
{
    public void Configure(EntityTypeBuilder<PurchaseInvoiceMaster> builder)
    {
        builder.ToTable("PurchaseInvoiceMasters");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.InvoiceNumber)
            .IsUnique();

        builder.Property(x => x.VendorInvoiceNumber)
            .HasMaxLength(50);

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.TaxAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.NetAmount)
            .HasPrecision(18, 4);

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PostedBy)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(x => x.Vendor)
            .WithMany(x => x.Invoices)
            .HasForeignKey(x => x.VendorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FiscalPeriod)
            .WithMany()
            .HasForeignKey(x => x.FiscalPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.PurchaseInvoiceMaster)
            .HasForeignKey(x => x.PurchaseInvoiceMasterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
