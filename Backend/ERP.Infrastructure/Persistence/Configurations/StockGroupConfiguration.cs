using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class StockGroupConfiguration : IEntityTypeConfiguration<StockGroup>
{
    public void Configure(EntityTypeBuilder<StockGroup> builder)
    {
        builder.ToTable("StockGroups");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.GroupCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.GroupCode)
            .IsUnique();

        builder.Property(x => x.GroupNameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.GroupNameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.IsDetail)
            .IsRequired()
            .HasDefaultValue(true);

        // Self-referencing relationship
        builder.HasOne(x => x.ParentGroup)
            .WithMany(x => x.SubGroups)
            .HasForeignKey(x => x.ParentGroupId)
            .OnDelete(DeleteBehavior.Restrict);

        // Accounting Integration Relationships
        builder.HasOne(x => x.InventoryAccount)
            .WithMany()
            .HasForeignKey(x => x.InventoryAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SalesAccount)
            .WithMany()
            .HasForeignKey(x => x.SalesAccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CostOfGoodsSoldAccount)
            .WithMany()
            .HasForeignKey(x => x.CostOfGoodsSoldAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
