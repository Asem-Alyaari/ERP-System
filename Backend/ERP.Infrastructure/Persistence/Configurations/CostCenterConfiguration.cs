using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class CostCenterConfiguration : IEntityTypeConfiguration<CostCenter>
{
    public void Configure(EntityTypeBuilder<CostCenter> builder)
    {
        builder.ToTable("CostCenters");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CostCenterCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(x => x.CostCenterCode)
            .IsUnique();

        builder.Property(x => x.CostCenterNameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CostCenterNameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.IsDetail)
            .IsRequired()
            .HasDefaultValue(true);

        // Self-referencing relationship
        builder.HasOne(x => x.ParentCostCenter)
            .WithMany(x => x.SubCostCenters)
            .HasForeignKey(x => x.ParentCostCenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
