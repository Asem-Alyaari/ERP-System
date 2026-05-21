using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class InventoryTransactionLineConfiguration : IEntityTypeConfiguration<InventoryTransactionLine>
{
    public void Configure(EntityTypeBuilder<InventoryTransactionLine> builder)
    {
        builder.ToTable("InventoryTransactionLines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
            .HasPrecision(18, 4);

        builder.Property(x => x.ConversionRate)
            .HasPrecision(18, 4);

        builder.Property(x => x.BaseQuantity)
            .HasPrecision(18, 4);

        builder.Property(x => x.Price)
            .HasPrecision(18, 4);

        builder.Property(x => x.Total)
            .HasPrecision(18, 4);

        builder.Property(x => x.BatchNumber)
            .HasMaxLength(100);

        // Relationships
        builder.HasOne(x => x.Item)
            .WithMany()
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Unit)
            .WithMany()
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
