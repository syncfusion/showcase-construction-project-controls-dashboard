using System.ComponentModel.DataAnnotations;

namespace Construction.Core.Entities;

/// <summary>
/// Represents a Request for Information
/// </summary>
public class RFI : BaseEntity
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Number { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public RFIStatus Status { get; set; }

    [MaxLength(100)]
    public string? SubmittedBy { get; set; }

    public DateTime? SubmittedDate { get; set; }

    [MaxLength(100)]
    public string? AssignedTo { get; set; }

    public DateTime? ResponseDate { get; set; }

    public string? Response { get; set; }

    [MaxLength(100)]
    public string? Discipline { get; set; }

    [MaxLength(200)]
    public string? Impact { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}
