using ERP.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ERP.Infrastructure.Persistence.Configurations;

public class ExpenseBillLineConfiguration : IEntityTypeConfiguration<ExpenseBillLine>
{
    public void Configure(EntityTypeBuilder<ExpenseBillLine> builder)
    {
        builder.ToTable("ExpenseBillLines");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 4);

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(x => x.ExpenseBillMaster)
            .WithMany(x => x.Lines)
            .HasForeignKey(x => x.ExpenseBillMasterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CostCenter)
            .WithMany()
            .HasForeignKey(x => x.CostCenterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
