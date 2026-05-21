using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemCode)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.ItemCode)
            .IsUnique();

        builder.Property(x => x.ItemNameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ItemNameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Barcode)
            .HasMaxLength(100);

        builder.HasIndex(x => x.Barcode)
            .IsUnique()
            .HasFilter("[Barcode] IS NOT NULL");

        builder.Property(x => x.DefaultPurchasePrice)
            .HasPrecision(18, 4);

        builder.Property(x => x.SalesPrice)
            .HasPrecision(18, 4);

        builder.Property(x => x.ReorderLevel)
            .HasPrecision(18, 4);

        builder.Property(x => x.MinimumQuantity)
            .HasPrecision(18, 4);

        builder.Property(x => x.MaximumQuantity)
            .HasPrecision(18, 4);

        builder.Property(x => x.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Relationships
        builder.HasOne(x => x.StockGroup)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.StockGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.SafetyStockUnit)
            .WithMany()
            .HasForeignKey(x => x.SafetyStockUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.ItemUnits)
            .WithOne(x => x.Item)
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.ItemUnits)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Batches)
            .WithOne(x => x.Item)
            .HasForeignKey(x => x.ItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Batches)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
