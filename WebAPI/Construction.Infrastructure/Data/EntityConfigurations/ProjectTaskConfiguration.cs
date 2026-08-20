using Construction.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Construction.Infrastructure.Data.EntityConfigurations;

public class ProjectTaskConfiguration : IEntityTypeConfiguration<ProjectTask>
{
    public void Configure(EntityTypeBuilder<ProjectTask> builder)
    {
        builder.ToTable("Tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(2000);

        builder.Property(t => t.StartDate)
            .IsRequired();

        builder.Property(t => t.EndDate)
            .IsRequired();

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.Progress)
            .IsRequired();

        builder.Property(t => t.AssignedTo)
            .HasMaxLength(100);

        builder.Property(t => t.Dependencies)
            .HasMaxLength(500);

        builder.Property(t => t.CreatedDate)
            .IsRequired();

        builder.Property(t => t.ModifiedDate);

        // Self-referencing relationship for parent-child tasks
        builder.HasOne(t => t.ParentTask)
            .WithMany(t => t.SubTasks)
            .HasForeignKey(t => t.ParentTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes for query performance
        builder.HasIndex(t => t.ProjectId);
        builder.HasIndex(t => t.Status);
        builder.HasIndex(t => t.StartDate);
        builder.HasIndex(t => t.EndDate);
    }
}
