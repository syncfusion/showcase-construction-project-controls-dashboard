using Construction.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Construction.Infrastructure.Data.EntityConfigurations;

public class ChangeOrderConfiguration : IEntityTypeConfiguration<ChangeOrder>
{
    public void Configure(EntityTypeBuilder<ChangeOrder> builder)
    {
        builder.ToTable("ChangeOrders");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Number)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(c => c.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.RequestedBy)
            .HasMaxLength(100);

        builder.Property(c => c.ApprovedBy)
            .HasMaxLength(100);

        builder.Property(c => c.Justification);

        builder.Property(c => c.CreatedDate)
            .IsRequired();

        builder.Property(c => c.ModifiedDate);

        builder.HasOne(c => c.Project)
            .WithMany(p => p.ChangeOrders)
            .HasForeignKey(c => c.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.ProjectId);
        builder.HasIndex(c => new { c.ProjectId, c.Number }).IsUnique();
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.RequestDate);
    }
}
