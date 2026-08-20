using Construction.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Construction.Infrastructure.Data.EntityConfigurations;

public class RiskConfiguration : IEntityTypeConfiguration<Risk>
{
    public void Configure(EntityTypeBuilder<Risk> builder)
    {
        builder.ToTable("Risks");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.ProjectId)
            .IsRequired();

        builder.Property(r => r.Number)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(r => r.Number)
            .IsUnique();

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(r => r.Description)
            .HasMaxLength(2000);

        builder.Property(r => r.Severity)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.Probability)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.ImpactType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.ImpactDescription)
            .HasMaxLength(500);

        builder.Property(r => r.ImpactCost)
            .HasPrecision(18, 2);

        builder.Property(r => r.Owner)
            .HasMaxLength(100);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.MitigationPlan)
            .HasMaxLength(2000);

        builder.Property(r => r.IdentifiedDate)
            .IsRequired();

        builder.Property(r => r.CreatedDate)
            .IsRequired();

        builder.Property(r => r.ModifiedDate);

        builder.HasOne(r => r.Project)
            .WithMany(p => p.Risks)
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.ProjectId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.Severity);
        builder.HasIndex(r => r.IdentifiedDate);
    }
}
