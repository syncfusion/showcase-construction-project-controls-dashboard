using System.ComponentModel.DataAnnotations;

namespace Construction.Core.Entities;

/// <summary>
/// Represents a project milestone
/// </summary>
public class Milestone : BaseEntity
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public TaskStatus Status { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
}
