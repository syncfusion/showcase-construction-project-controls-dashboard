using Construction.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Construction.Infrastructure.Data.EntityConfigurations;

public class SubmittalConfiguration : IEntityTypeConfiguration<Submittal>
{
    public void Configure(EntityTypeBuilder<Submittal> builder)
    {
        builder.ToTable("Submittals");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Number)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(s => s.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.SubmittedBy)
            .HasMaxLength(100);

        builder.Property(s => s.ReviewedBy)
            .HasMaxLength(100);

        builder.Property(s => s.Comments);

        builder.Property(s => s.Discipline)
            .HasMaxLength(100);

        builder.Property(s => s.SpecificationSection)
            .HasMaxLength(100);

        builder.Property(s => s.SubmittalType)
            .HasMaxLength(50);

        builder.Property(s => s.CreatedDate)
            .IsRequired();

        builder.Property(s => s.ModifiedDate);

        builder.HasOne(s => s.Project)
            .WithMany(p => p.Submittals)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.ProjectId);
        builder.HasIndex(s => new { s.ProjectId, s.Number }).IsUnique();
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.SubmittedDate);
        builder.HasIndex(s => s.Title);
    }
}
