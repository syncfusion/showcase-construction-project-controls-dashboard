using Construction.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Construction.Infrastructure.Data.EntityConfigurations;

public class SiteLocationConfiguration : IEntityTypeConfiguration<SiteLocation>
{
    public void Configure(EntityTypeBuilder<SiteLocation> builder)
    {
        builder.ToTable("Locations");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Description)
            .HasMaxLength(2000);

        builder.Property(l => l.Latitude)
            .IsRequired();

        builder.Property(l => l.Longitude)
            .IsRequired();

        builder.Property(l => l.LocationType)
            .HasMaxLength(100);

        builder.Property(l => l.CreatedDate)
            .IsRequired();

        builder.Property(l => l.ModifiedDate);

        builder.HasOne(l => l.Project)
            .WithMany(p => p.Locations)
            .HasForeignKey(l => l.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(l => l.ProjectId);
        builder.HasIndex(l => new { l.ProjectId, l.Name });
    }
}
