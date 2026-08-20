using System.ComponentModel.DataAnnotations;

namespace Construction.Core.Entities;

/// <summary>
/// Represents a document or file associated with a construction project
/// </summary>
public class Document : BaseEntity
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    public Guid FileId { get; set; } // Unique file identifier

    [Required]
    [MaxLength(500)]
    public string FileName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FileType { get; set; } = string.Empty; // e.g., "PDF", "DWG", "XLSX"

    public long FileSize { get; set; } // Size in bytes

    [MaxLength(200)]
    public string? DocumentType { get; set; } // e.g., "Contract", "Drawing", "Report"

    public string? Description { get; set; }

    [MaxLength(100)]
    public string? UploadedBy { get; set; }

    public DateTime? UploadDate { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}
