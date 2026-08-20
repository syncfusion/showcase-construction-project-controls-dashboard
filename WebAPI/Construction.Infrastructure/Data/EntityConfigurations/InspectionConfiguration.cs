using Construction.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Construction.Infrastructure.Data.EntityConfigurations;

public class InspectionConfiguration : IEntityTypeConfiguration<Inspection>
{
    public void Configure(EntityTypeBuilder<Inspection> builder)
    {
        builder.ToTable("Inspections");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Type)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(i => i.ScheduledDate)
            .IsRequired();

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(i => i.Inspector)
            .HasMaxLength(100);

        builder.Property(i => i.Notes);
        builder.Property(i => i.Findings);

        builder.Property(i => i.CreatedDate)
            .IsRequired();

        builder.Property(i => i.ModifiedDate);

        builder.HasOne(i => i.Project)
            .WithMany(p => p.Inspections)
            .HasForeignKey(i => i.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Location)
            .WithMany(l => l.Inspections)
            .HasForeignKey(i => i.LocationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(i => i.ProjectId);
        builder.HasIndex(i => i.Status);
        builder.HasIndex(i => i.ScheduledDate);
    }
}
