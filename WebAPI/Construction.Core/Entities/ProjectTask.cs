using System.ComponentModel.DataAnnotations;

namespace Construction.Core.Entities;

/// <summary>
/// Represents a task within a construction project (used for Gantt Chart)
/// </summary>
public class ProjectTask : BaseEntity
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public TaskStatus Status { get; set; }

    [Range(0, 100)]
    public int Progress { get; set; }

    [MaxLength(100)]
    public string? AssignedTo { get; set; }

    public int? ParentTaskId { get; set; }

    [MaxLength(500)]
    public string? Dependencies { get; set; } // Comma-separated task IDs

    // Navigation properties
    public Project Project { get; set; } = null!;
    public ProjectTask? ParentTask { get; set; }
    public ICollection<ProjectTask> SubTasks { get; set; } = new List<ProjectTask>();
}
