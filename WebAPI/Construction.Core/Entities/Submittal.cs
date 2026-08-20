using System.ComponentModel.DataAnnotations;

namespace Construction.Core.Entities;

/// <summary>
/// Represents a construction submittal
/// </summary>
public class Submittal : BaseEntity
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Short, human-readable name of the submittal — the column shown in the UI's
    /// Submittals table (the frontend <c>SubmittalSummaryDto.title</c> field).
    /// Distinct from <see cref="Description"/>, which holds the longer narrative.
    /// </summary>
    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public SubmittalStatus Status { get; set; }

    [MaxLength(100)]
    public string? SubmittedBy { get; set; }

    public DateTime? SubmittedDate { get; set; }

    [MaxLength(100)]
    public string? ReviewedBy { get; set; }

    public DateTime? ReviewDate { get; set; }

    public string? Comments { get; set; }

    [MaxLength(100)]
    public string? Discipline { get; set; }

    [MaxLength(100)]
    public string? SpecificationSection { get; set; }

    [MaxLength(50)]
    public string? SubmittalType { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}
