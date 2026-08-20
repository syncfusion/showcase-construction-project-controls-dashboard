using System.ComponentModel.DataAnnotations;

namespace Construction.Core.Entities;

/// <summary>
/// Represents a construction site inspection (used for Scheduler)
/// </summary>
public class Inspection : BaseEntity
{
    [Required]
    public int ProjectId { get; set; }

    public int? LocationId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Type { get; set; } = string.Empty; // e.g., "Safety", "Quality", "Final"

    [Required]
    public DateTime ScheduledDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    [Required]
    public InspectionStatus Status { get; set; }

    [MaxLength(100)]
    public string? Inspector { get; set; }

    public string? Notes { get; set; }

    public string? Findings { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public SiteLocation? Location { get; set; }
}
