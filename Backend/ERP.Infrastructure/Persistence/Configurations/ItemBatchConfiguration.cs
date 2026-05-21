using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class ItemBatchConfiguration : IEntityTypeConfiguration<ItemBatch>
{
    public void Configure(EntityTypeBuilder<ItemBatch> builder)
    {
        builder.ToTable("ItemBatches");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.BatchNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.PurchasePrice)
            .HasPrecision(18, 4);

        builder.Property(x => x.QuantityOnHand)
            .HasPrecision(18, 4);

        // Composite Index for ItemId and BatchNumber
        builder.HasIndex(x => new { x.ItemId, x.BatchNumber })
            .IsUnique();
    }
}
