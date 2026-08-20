using System.ComponentModel.DataAnnotations;

namespace Construction.Core.Entities;

/// <summary>
/// Represents a change order for a construction project
/// </summary>
public class ChangeOrder : BaseEntity
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    [MaxLength(50)]
    public string Number { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public ChangeOrderStatus Status { get; set; }

    [MaxLength(100)]
    public string? RequestedBy { get; set; }

    public DateTime? RequestDate { get; set; }

    [MaxLength(100)]
    public string? ApprovedBy { get; set; }

    public DateTime? ApprovalDate { get; set; }

    public string? Justification { get; set; }

    public int? ImpactDays { get; set; } // Schedule impact in days

    // Navigation properties
    public Project Project { get; set; } = null!;
}
