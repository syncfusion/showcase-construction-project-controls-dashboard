using System.ComponentModel.DataAnnotations;

namespace Construction.Core.Entities;

/// <summary>
/// Represents a construction project
/// </summary>
public class Project : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    [Required]
    public ProjectStatus Status { get; set; }

    [MaxLength(500)]
    public string? Location { get; set; }

    public decimal Budget { get; set; }

    [Range(0, 100)]
    public int Progress { get; set; }

    [MaxLength(100)]
    public string? Manager { get; set; }

    // Navigation properties
    public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    public ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    public ICollection<SiteLocation> Locations { get; set; } = new List<SiteLocation>();
    public ICollection<RFI> RFIs { get; set; } = new List<RFI>();
    public ICollection<Submittal> Submittals { get; set; } = new List<Submittal>();
    public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    public ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public ICollection<ChangeOrder> ChangeOrders { get; set; } = new List<ChangeOrder>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Risk> Risks { get; set; } = new List<Risk>();
}
