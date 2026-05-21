using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class SalesInvoiceMasterConfiguration : IEntityTypeConfiguration<SalesInvoiceMaster>
{
    public void Configure(EntityTypeBuilder<SalesInvoiceMaster> builder)
    {
        builder.ToTable("SalesInvoiceMasters");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InvoiceNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.InvoiceNumber)
            .IsUnique();

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
        builder.HasOne(x => x.Customer)
            .WithMany(x => x.Invoices)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.FiscalPeriod)
            .WithMany()
            .HasForeignKey(x => x.FiscalPeriodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.SalesInvoiceMaster)
            .HasForeignKey(x => x.SalesInvoiceMasterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
