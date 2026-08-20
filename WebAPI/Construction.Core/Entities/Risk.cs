using System.ComponentModel.DataAnnotations;

namespace Construction.Core.Entities;

/// <summary>
/// Represents a project risk or issue.
/// </summary>
public class Risk : BaseEntity
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Number { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    [Required]
    public RiskSeverity Severity { get; set; }

    [Required]
    public RiskProbability Probability { get; set; }

    [Required]
    public RiskImpactType ImpactType { get; set; }

    [MaxLength(500)]
    public string? ImpactDescription { get; set; }

    public decimal? ImpactCost { get; set; }

    public int? ImpactDays { get; set; }

    [MaxLength(100)]
    public string? Owner { get; set; }

    [Required]
    public RiskStatus Status { get; set; }

    [MaxLength(2000)]
    public string? MitigationPlan { get; set; }

    [Required]
    public DateTime IdentifiedDate { get; set; }

    public DateTime? TargetResolutionDate { get; set; }

    public DateTime? ClosedDate { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}
