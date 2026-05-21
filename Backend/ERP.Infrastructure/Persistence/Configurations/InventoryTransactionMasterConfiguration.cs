using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class InventoryTransactionMasterConfiguration : IEntityTypeConfiguration<InventoryTransactionMaster>
{
    public void Configure(EntityTypeBuilder<InventoryTransactionMaster> builder)
    {
        builder.ToTable("InventoryTransactionMasters");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.DocumentNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.DocumentNumber)
            .IsUnique();

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ApprovedBy)
            .HasMaxLength(100);

        // One-to-many relationship with Lines
        builder.HasMany(x => x.Lines)
            .WithOne(x => x.InventoryTransactionMaster)
            .HasForeignKey(x => x.InventoryTransactionMasterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
