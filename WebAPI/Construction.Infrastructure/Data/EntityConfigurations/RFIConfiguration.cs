using Construction.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Construction.Infrastructure.Data.EntityConfigurations;

public class RFIConfiguration : IEntityTypeConfiguration<RFI>
{
    public void Configure(EntityTypeBuilder<RFI> builder)
    {
        builder.ToTable("RFIs");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Number)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(r => r.Description)
            .IsRequired();

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(r => r.SubmittedBy)
            .HasMaxLength(100);

        builder.Property(r => r.AssignedTo)
            .HasMaxLength(100);

        builder.Property(r => r.Response);

        builder.Property(r => r.Discipline)
            .HasMaxLength(100);

        builder.Property(r => r.Impact)
            .HasMaxLength(200);

        builder.Property(r => r.CreatedDate)
            .IsRequired();

        builder.Property(r => r.ModifiedDate);

        builder.HasOne(r => r.Project)
            .WithMany(p => p.RFIs)
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.ProjectId);
        builder.HasIndex(r => new { r.ProjectId, r.Number }).IsUnique();
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.SubmittedDate);
    }
}
