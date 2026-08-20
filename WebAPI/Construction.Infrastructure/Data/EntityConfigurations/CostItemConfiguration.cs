using Construction.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Construction.Infrastructure.Data.EntityConfigurations;

public class CostItemConfiguration : IEntityTypeConfiguration<CostItem>
{
    public void Configure(EntityTypeBuilder<CostItem> builder)
    {
        builder.ToTable("CostItems");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(c => c.Vendor)
            .HasMaxLength(100);

        builder.Property(c => c.Reference)
            .HasMaxLength(100);

        builder.Property(c => c.CreatedDate)
            .IsRequired();

        builder.Property(c => c.ModifiedDate);

        builder.HasOne(c => c.Budget)
            .WithMany(b => b.CostItems)
            .HasForeignKey(c => c.BudgetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.BudgetId);
        builder.HasIndex(c => c.Date);
    }
}
