using System.ComponentModel.DataAnnotations;

namespace Construction.Core.Entities;

/// <summary>
/// Represents a budget category for a construction project
/// </summary>
public class Budget : BaseEntity
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Category { get; set; } = string.Empty; // e.g., "Labor", "Materials", "Equipment"

    public string? Description { get; set; }

    [Required]
    public decimal AllocatedAmount { get; set; }

    public decimal SpentAmount { get; set; }

    // Navigation properties
    public Project Project { get; set; } = null!;
    public ICollection<CostItem> CostItems { get; set; } = new List<CostItem>();
}
