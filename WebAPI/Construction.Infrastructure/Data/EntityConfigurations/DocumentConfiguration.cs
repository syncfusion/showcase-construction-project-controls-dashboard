using Construction.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Construction.Infrastructure.Data.EntityConfigurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.FileId)
            .IsRequired();

        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.FileType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(d => d.DocumentType)
            .HasMaxLength(200);

        builder.Property(d => d.UploadedBy)
            .HasMaxLength(100);

        builder.Property(d => d.CreatedDate)
            .IsRequired();

        builder.Property(d => d.ModifiedDate);

        builder.HasOne(d => d.Project)
            .WithMany(p => p.Documents)
            .HasForeignKey(d => d.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(d => d.ProjectId);
        builder.HasIndex(d => d.FileId).IsUnique();
        builder.HasIndex(d => d.UploadDate);
    }
}
