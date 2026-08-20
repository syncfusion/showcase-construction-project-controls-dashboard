using Construction.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Construction.Infrastructure.Data.EntityConfigurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("Budgets");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Category)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Description)
            .HasMaxLength(2000);

        builder.Property(b => b.AllocatedAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(b => b.SpentAmount)
            .HasPrecision(18, 2);

        builder.Property(b => b.CreatedDate)
            .IsRequired();

        builder.Property(b => b.ModifiedDate);

        // Relationship with CostItems
        builder.HasMany(b => b.CostItems)
            .WithOne(c => c.Budget)
            .HasForeignKey(c => c.BudgetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(b => b.ProjectId);
    }
}
